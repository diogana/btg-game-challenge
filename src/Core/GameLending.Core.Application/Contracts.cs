using GameLending.Core.Domain;
namespace GameLending.Core.Application;

public sealed record Page<T>(IReadOnlyList<T> Items, int Total, int PageNumber, int PageSize);
public sealed record FriendView(Guid Id, string Name, string? Email, bool IsActive)
{ public FriendView() : this(default, "", null, false) { } }
public sealed record GameView(Guid Id, string Title, string[] Platforms, bool IsActive, string Status,
    Guid? LoanId, Guid? FriendId, string? FriendName)
{ public GameView() : this(default, "", [], false, "", null, null, null) { } }
public sealed record LoanView(Guid Id, Guid GameId, string GameTitle, Guid FriendId, string FriendName,
    DateTimeOffset LoanedAt, DateTimeOffset? ReturnedAt)
{ public LoanView() : this(default, default, "", default, "", default, null) { } }
public sealed record Statistics(int TotalGames, int AvailableGames, int BorrowedGames, int TotalFriends, int ActiveLoans);
public sealed record SaveFriend(string Name, string? Email);
public sealed record SaveGame(string Title, string[] Platforms);
public sealed record CreateLoan(Guid GameId, Guid FriendId);
public sealed class NotFoundException(string resource) : Exception($"{resource} was not found.");

public interface IGameRepository
{
    Task<Game?> Get(Guid id, CancellationToken ct);
    Task<Game?> Lock(Guid id, CancellationToken ct);
    void Add(Game game);
}
public interface IFriendRepository
{
    Task<Friend?> Get(Guid id, CancellationToken ct);
    Task<Friend?> Lock(Guid id, CancellationToken ct);
    void Add(Friend friend);
}
public interface ILoanRepository
{
    Task<Loan?> Get(Guid id, CancellationToken ct);
    Task<Loan?> Lock(Guid id, CancellationToken ct);
    Task<bool> HasActiveGame(Guid gameId, CancellationToken ct);
    Task<bool> HasActiveFriend(Guid friendId, CancellationToken ct);
    void Add(Loan loan);
}
public interface IUnitOfWork
{
    Task<T> Execute<T>(Func<Task<T>> action, CancellationToken ct);
}
public interface ICatalogQueries
{
    Task<Page<FriendView>> Friends(string? search, int page, int size, bool includeInactive, CancellationToken ct);
    Task<FriendView?> Friend(Guid id, CancellationToken ct);
    Task<Page<GameView>> Games(string? search, string? status, int page, int size, bool includeInactive, CancellationToken ct);
    Task<GameView?> Game(Guid id, CancellationToken ct);
    Task<Page<LoanView>> Loans(bool activeOnly, Guid? friendId, Guid? gameId, Guid[]? gameIds, int page, int size, CancellationToken ct);
    Task<LoanView?> Loan(Guid id, CancellationToken ct);
    Task<Statistics> Statistics(CancellationToken ct);
}
