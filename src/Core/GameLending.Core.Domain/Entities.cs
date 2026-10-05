namespace GameLending.Core.Domain;

public sealed class Game
{
    private Game() { }
    public Guid Id { get; private set; }
    public string Title { get; private set; } = "";
    public string[] Platforms { get; private set; } = [];
    public string[] Genres { get; private set; } = [];
    public string? Developer { get; private set; }
    public DateOnly? ReleaseDate { get; private set; }
    public decimal? Rating { get; private set; }
    public long? Votes { get; private set; }
    public decimal? SourcePrice { get; private set; }
    public string? SourceUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public static Game Create(string title, string[] platforms, DateTimeOffset now)
    {
        var game = new Game { Id = Guid.NewGuid(), CreatedAt = now };
        game.Update(title, platforms, now);
        return game;
    }
    public void Update(string title, string[] platforms, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 300)
            throw new DomainException("Game title must contain between 1 and 300 characters.");
        Title = title.Trim();
        Platforms = platforms.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToArray();
        UpdatedAt = now;
    }
    public void SetCatalogData(CatalogSource source, string[] genres, string? developer,
        DateOnly? releaseDate, decimal? rating, long? votes, decimal? price)
    {
        SourceUrl = source.Url; Genres = genres; Developer = developer;
        ReleaseDate = releaseDate; Rating = rating; Votes = votes; SourcePrice = price;
    }
    public void Deactivate(bool hasActiveLoan, DateTimeOffset now)
    {
        if (hasActiveLoan) throw new DomainException("Return the game before deactivating it.");
        IsActive = false; UpdatedAt = now;
    }
}

public sealed record CatalogSource
{
    public string Url { get; }
    public CatalogSource(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != "https")
            throw new DomainException("Catalog URL must be an absolute HTTPS URL.");
        Url = uri.AbsoluteUri;
    }
}

public sealed class Friend
{
    private Friend() { }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = "";
    public string? Email { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public static Friend Create(string name, string? email, DateTimeOffset now)
    {
        var friend = new Friend { Id = Guid.NewGuid(), CreatedAt = now };
        friend.Update(name, email, now); return friend;
    }
    public void Update(string name, string? email, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 150)
            throw new DomainException("Friend name must contain between 1 and 150 characters.");
        if (!string.IsNullOrWhiteSpace(email) && (!System.Net.Mail.MailAddress.TryCreate(email, out _) || email.Length > 254))
            throw new DomainException("Email is invalid.");
        Name = name.Trim(); Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(); UpdatedAt = now;
    }
    public void Deactivate(bool hasActiveLoan, DateTimeOffset now)
    {
        if (hasActiveLoan) throw new DomainException("Return borrowed games before deactivating this friend.");
        IsActive = false; UpdatedAt = now;
    }
}

public sealed class Loan
{
    private Loan() { }
    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }
    public Guid FriendId { get; private set; }
    public DateTimeOffset LoanedAt { get; private set; }
    public DateTimeOffset? ReturnedAt { get; private set; }
    public bool IsActive => ReturnedAt is null;
    public static Loan Create(Game game, Friend friend, bool hasActiveLoan, DateTimeOffset now)
    {
        LendingPolicy.EnsureCanBorrow(game, friend, hasActiveLoan);
        return new Loan { Id = Guid.NewGuid(), GameId = game.Id, FriendId = friend.Id, LoanedAt = now };
    }
    public void Return(DateTimeOffset now)
    {
        if (!IsActive) throw new DomainException("Loan has already been returned.");
        if (now < LoanedAt) throw new DomainException("Return date cannot precede loan date.");
        ReturnedAt = now;
    }
}

public static class LendingPolicy
{
    public static void EnsureCanBorrow(Game game, Friend friend, bool hasActiveLoan)
    {
        if (!game.IsActive) throw new DomainException("Inactive game cannot be loaned.");
        if (!friend.IsActive) throw new DomainException("Inactive friend cannot borrow.");
        if (hasActiveLoan) throw new DomainException("Game already has an active loan.");
    }
}
