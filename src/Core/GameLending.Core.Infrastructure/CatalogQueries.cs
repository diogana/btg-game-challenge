using GameLending.Core.Application;
using Microsoft.EntityFrameworkCore;
namespace GameLending.Core.Infrastructure;

public sealed class CatalogQueries(LendingDbContext db) : ICatalogQueries
{
    private IQueryable<FriendView> FriendRows => db.Friends.AsNoTracking().Select(x => new FriendView { Id = x.Id, Name = x.Name, Email = x.Email, IsActive = x.IsActive });
    private IQueryable<GameView> GameRows =>
        from g in db.Games.AsNoTracking()
        join l in db.Loans.Where(x => x.ReturnedAt == null) on g.Id equals l.GameId into active
        from l in active.DefaultIfEmpty()
        join f in db.Friends on l.FriendId equals f.Id into borrowers
        from f in borrowers.DefaultIfEmpty()
        select new GameView
        {
            Id = g.Id,
            Title = g.Title,
            Platforms = g.Platforms,
            IsActive = g.IsActive,
            Status = !g.IsActive ? "Inactive" : l != null ? "Borrowed" : "Available",
            LoanId = l != null ? l.Id : null,
            FriendId = f != null ? f.Id : null,
            FriendName = f != null ? f.Name : null
        };
    private IQueryable<LoanView> LoanRows =>
        from l in db.Loans.AsNoTracking()
        join g in db.Games on l.GameId equals g.Id
        join f in db.Friends on l.FriendId equals f.Id
        select new LoanView { Id = l.Id, GameId = g.Id, GameTitle = g.Title, FriendId = f.Id, FriendName = f.Name, LoanedAt = l.LoanedAt, ReturnedAt = l.ReturnedAt };
    private static async Task<Page<T>> Paginate<T>(IQueryable<T> query, int page, int size, CancellationToken ct)
    {
        page = Math.Max(1, page); size = Math.Clamp(size, 1, 100);
        var count = await query.CountAsync(ct);
        return new(await query.Skip((page - 1) * size).Take(size).ToListAsync(ct), count, page, size);
    }
    public Task<Page<FriendView>> Friends(string? search, int page, int size, bool includeInactive, CancellationToken ct)
    {
        var rows = FriendRows.Where(x => includeInactive || x.IsActive);
        if (!string.IsNullOrWhiteSpace(search)) rows = rows.Where(x => EF.Functions.ILike(x.Name, "%" + search + "%"));
        return Paginate(rows.OrderBy(x => x.Name).ThenBy(x => x.Id), page, size, ct);
    }
    public Task<FriendView?> Friend(Guid id, CancellationToken ct) => FriendRows.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Page<GameView>> Games(string? search, string? status, int page, int size, bool includeInactive, CancellationToken ct)
    {
        var rows = GameRows.Where(x => includeInactive || x.IsActive);
        if (!string.IsNullOrWhiteSpace(search)) rows = rows.Where(x => EF.Functions.ILike(x.Title, "%" + search + "%"));
        if (!string.IsNullOrWhiteSpace(status)) rows = rows.Where(x => x.Status == status);
        return Paginate(rows.OrderBy(x => x.Title).ThenBy(x => x.Id), page, size, ct);
    }
    public Task<GameView?> Game(Guid id, CancellationToken ct) => GameRows.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Page<LoanView>> Loans(bool activeOnly, Guid? friendId, Guid? gameId, Guid[]? gameIds, int page, int size, CancellationToken ct)
    {
        var rows = LoanRows.Where(x => !activeOnly || x.ReturnedAt == null);
        if (friendId.HasValue) rows = rows.Where(x => x.FriendId == friendId);
        if (gameId.HasValue) rows = rows.Where(x => x.GameId == gameId);
        if (gameIds is { Length: > 0 }) rows = rows.Where(x => gameIds.Contains(x.GameId));
        return Paginate(rows.OrderByDescending(x => x.LoanedAt).ThenBy(x => x.Id), page, size, ct);
    }
    public Task<LoanView?> Loan(Guid id, CancellationToken ct) => LoanRows.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<Statistics> Statistics(CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
        var games = await db.Games.CountAsync(x => x.IsActive, ct);
        var borrowed = await db.Loans.CountAsync(x => x.ReturnedAt == null, ct);
        var friends = await db.Friends.CountAsync(x => x.IsActive, ct);
        await tx.CommitAsync(ct); return new(games, games - borrowed, borrowed, friends, borrowed);
    }
}
