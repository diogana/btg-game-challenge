using GameLending.Core.Application;
using GameLending.Core.Domain;
using Microsoft.EntityFrameworkCore;
namespace GameLending.Core.Infrastructure;

public sealed class GameRepository(LendingDbContext db) : IGameRepository
{
    public Task<Game?> Get(Guid id, CancellationToken ct) => db.Games.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<Game?> Lock(Guid id, CancellationToken ct) => (await db.Games.FromSqlInterpolated($"SELECT * FROM games WHERE id={id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
    public void Add(Game game) => db.Games.Add(game);
}
public sealed class FriendRepository(LendingDbContext db) : IFriendRepository
{
    public Task<Friend?> Get(Guid id, CancellationToken ct) => db.Friends.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<Friend?> Lock(Guid id, CancellationToken ct) => (await db.Friends.FromSqlInterpolated($"SELECT * FROM friends WHERE id={id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
    public void Add(Friend friend) => db.Friends.Add(friend);
}
public sealed class LoanRepository(LendingDbContext db) : ILoanRepository
{
    public Task<Loan?> Get(Guid id, CancellationToken ct) => db.Loans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<Loan?> Lock(Guid id, CancellationToken ct) => (await db.Loans.FromSqlInterpolated($"SELECT * FROM loans WHERE id={id} FOR UPDATE").ToListAsync(ct)).SingleOrDefault();
    public Task<bool> HasActiveGame(Guid id, CancellationToken ct) => db.Loans.AnyAsync(x => x.GameId == id && x.ReturnedAt == null, ct);
    public Task<bool> HasActiveFriend(Guid id, CancellationToken ct) => db.Loans.AnyAsync(x => x.FriendId == id && x.ReturnedAt == null, ct);
    public void Add(Loan loan) => db.Loans.Add(loan);
}
public sealed class UnitOfWork(LendingDbContext db) : IUnitOfWork
{
    public async Task<T> Execute<T>(Func<Task<T>> action, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var result = await action();
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return result;
    }
}
