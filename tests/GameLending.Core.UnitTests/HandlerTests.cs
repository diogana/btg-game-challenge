using GameLending.Core.Application;
using GameLending.Core.Domain;
namespace GameLending.Core.UnitTests;

public sealed class HandlerTests
{
    [Fact]
    public async Task MissingFriendStopsLoanBeforeGameLookup()
    {
        var store = new Store(); var handler = new LoanCommands(store, store, store, store, TimeProvider.System);
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Create(new(Guid.NewGuid(), Guid.NewGuid()), default));
        Assert.False(store.GameLocked); Assert.Empty(store.Loans);
    }
    [Fact]
    public async Task CreatesLoanAfterLockingFriendAndGame()
    {
        var store = new Store { Friend = Friend.Create("Alice", null, DateTimeOffset.UtcNow), Game = Game.Create("Game", [], DateTimeOffset.UtcNow) };
        var handler = new LoanCommands(store, store, store, store, TimeProvider.System);
        var id = await handler.Create(new(store.Game.Id, store.Friend.Id), default);
        Assert.Equal(id, Assert.Single(store.Loans).Id); Assert.True(store.GameLocked);
    }
    private sealed class Store : IGameRepository, IFriendRepository, ILoanRepository, IUnitOfWork
    {
        public Game? Game; public Friend? Friend; public bool GameLocked; public List<Loan> Loans = [];
        Task<Game?> IGameRepository.Get(Guid id, CancellationToken ct) => Task.FromResult(Game);
        Task<Game?> IGameRepository.Lock(Guid id, CancellationToken ct) { GameLocked = true; return Task.FromResult(Game); }
        Task<Friend?> IFriendRepository.Get(Guid id, CancellationToken ct) => Task.FromResult(Friend);
        Task<Friend?> IFriendRepository.Lock(Guid id, CancellationToken ct) => Task.FromResult(Friend);
        Task<Loan?> ILoanRepository.Get(Guid id, CancellationToken ct) => Task.FromResult(Loans.SingleOrDefault(x => x.Id == id));
        Task<Loan?> ILoanRepository.Lock(Guid id, CancellationToken ct) => Task.FromResult(Loans.SingleOrDefault(x => x.Id == id));
        public void Add(Game game) => Game = game; public void Add(Friend friend) => Friend = friend; public void Add(Loan loan) => Loans.Add(loan);
        public Task<bool> HasActiveGame(Guid id, CancellationToken ct) => Task.FromResult(Loans.Any(x => x.GameId == id && x.IsActive));
        public Task<bool> HasActiveFriend(Guid id, CancellationToken ct) => Task.FromResult(Loans.Any(x => x.FriendId == id && x.IsActive));
        public Task<T> Execute<T>(Func<Task<T>> action, CancellationToken ct) => action();
    }
}
