using GameLending.Core.Domain;
namespace GameLending.Core.Application;

public sealed class FriendCommands(IFriendRepository friends, ILoanRepository loans, IUnitOfWork work, TimeProvider clock)
{
    public Task<Guid> Create(SaveFriend input, CancellationToken ct) => work.Execute(() =>
    {
        var friend = Friend.Create(input.Name, input.Email, clock.GetUtcNow());
        friends.Add(friend); return Task.FromResult(friend.Id);
    }, ct);
    public Task<Guid> Update(Guid id, SaveFriend input, CancellationToken ct) => work.Execute(async () =>
    {
        var friend = await friends.Lock(id, ct) ?? throw new NotFoundException("Friend");
        friend.Update(input.Name, input.Email, clock.GetUtcNow()); return id;
    }, ct);
    public Task<Guid> Delete(Guid id, CancellationToken ct) => work.Execute(async () =>
    {
        var friend = await friends.Lock(id, ct) ?? throw new NotFoundException("Friend");
        friend.Deactivate(await loans.HasActiveFriend(id, ct), clock.GetUtcNow()); return id;
    }, ct);
}

public sealed class GameCommands(IGameRepository games, ILoanRepository loans, IUnitOfWork work, TimeProvider clock)
{
    public Task<Guid> Create(SaveGame input, CancellationToken ct) => work.Execute(() =>
    {
        var game = Game.Create(input.Title, input.Platforms, clock.GetUtcNow());
        games.Add(game); return Task.FromResult(game.Id);
    }, ct);
    public Task<Guid> Update(Guid id, SaveGame input, CancellationToken ct) => work.Execute(async () =>
    {
        var game = await games.Lock(id, ct) ?? throw new NotFoundException("Game");
        game.Update(input.Title, input.Platforms, clock.GetUtcNow()); return id;
    }, ct);
    public Task<Guid> Delete(Guid id, CancellationToken ct) => work.Execute(async () =>
    {
        var game = await games.Lock(id, ct) ?? throw new NotFoundException("Game");
        game.Deactivate(await loans.HasActiveGame(id, ct), clock.GetUtcNow()); return id;
    }, ct);
}

public sealed class LoanCommands(IGameRepository games, IFriendRepository friends, ILoanRepository loans,
    IUnitOfWork work, TimeProvider clock)
{
    public Task<Guid> Create(CreateLoan input, CancellationToken ct) => work.Execute(async () =>
    {
        var friend = await friends.Lock(input.FriendId, ct) ?? throw new NotFoundException("Friend");
        var game = await games.Lock(input.GameId, ct) ?? throw new NotFoundException("Game");
        var loan = Loan.Create(game, friend, await loans.HasActiveGame(game.Id, ct), clock.GetUtcNow());
        loans.Add(loan); return loan.Id;
    }, ct);
    public Task<Guid> Return(Guid id, CancellationToken ct) => work.Execute(async () =>
    {
        var existing = await loans.Get(id, ct) ?? throw new NotFoundException("Loan");
        await friends.Lock(existing.FriendId, ct);
        await games.Lock(existing.GameId, ct);
        var loan = await loans.Lock(id, ct) ?? throw new NotFoundException("Loan");
        loan.Return(clock.GetUtcNow()); return id;
    }, ct);
}
