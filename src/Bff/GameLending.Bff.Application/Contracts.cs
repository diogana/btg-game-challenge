namespace GameLending.Bff.Application;

public sealed record Page<T>(IReadOnlyList<T> Items, int Total, int PageNumber, int PageSize);
public sealed record FriendView(Guid Id, string Name, string? Email, bool IsActive);
public sealed record GameView(Guid Id, string Title, string[] Platforms, bool IsActive, string Status,
    Guid? LoanId, Guid? FriendId, string? FriendName);
public sealed record LoanView(Guid Id, Guid GameId, string GameTitle, Guid FriendId, string FriendName,
    DateTimeOffset LoanedAt, DateTimeOffset? ReturnedAt);
public sealed record Statistics(int TotalGames, int AvailableGames, int BorrowedGames, int TotalFriends, int ActiveLoans);
public sealed record SaveFriend(string Name, string? Email);
public sealed record SaveGame(string Title, string[] Platforms);
public sealed record CreateLoan(Guid GameId, Guid FriendId);
public sealed class NotFoundException(string resource) : Exception($"{resource} was not found.");


public sealed record LoginRequest(string ClientId, string ClientSecret);
public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);
public sealed record TokenValidationResponse(bool Valid, string? ClientId, DateTimeOffset Expiration, string[] Roles, string[] Scopes);
public sealed record DashboardView(Statistics Statistics, IReadOnlyList<LoanView> ActiveLoans, IReadOnlyList<LoanView> RecentLoans);
public sealed record LibraryView(Page<GameView> Games, IReadOnlyList<LoanView> ActiveLoans);
public sealed record GameDetailsView(GameView Game, Page<LoanView> History);
public sealed record FriendSummaryView(FriendView Friend, Page<LoanView> ActiveLoans, Page<LoanView> History);
public interface IBackendClient
{
    Task<T> Get<T>(string path, CancellationToken ct);
    Task<T> Send<T>(HttpMethod method, string path, object? body, CancellationToken ct);
    Task Delete(string path, CancellationToken ct);
}
public sealed class DownstreamException(int status, string message) : Exception(message) { public int Status { get; } = status; }
