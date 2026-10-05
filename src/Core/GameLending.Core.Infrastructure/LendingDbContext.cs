using GameLending.Core.Domain;
using Microsoft.EntityFrameworkCore;
namespace GameLending.Core.Infrastructure;

public sealed class ImportRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileHash { get; set; } = "";
    public string ParserVersion { get; set; } = "1";
    public string Status { get; set; } = "Running";
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public int Analyzed { get; set; }
    public int Imported { get; set; }
    public int Duplicates { get; set; }
    public int Rejected { get; set; }
    public int Warnings { get; set; }
}
public sealed class LendingDbContext(DbContextOptions<LendingDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Friend> Friends => Set<Friend>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<ImportRun> ImportRuns => Set<ImportRun>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        var game = model.Entity<Game>();
        game.ToTable("games"); game.HasKey(x => x.Id);
        game.Property(x => x.Title).HasMaxLength(300).IsRequired();
        game.Property(x => x.Rating).HasPrecision(6, 2);
        game.Property(x => x.SourcePrice).HasPrecision(12, 2);
        game.HasIndex(x => x.SourceUrl).IsUnique().HasFilter("source_url IS NOT NULL");
        game.HasIndex(x => x.Title);
        var friend = model.Entity<Friend>();
        friend.ToTable("friends"); friend.HasKey(x => x.Id);
        friend.Property(x => x.Name).HasMaxLength(150).IsRequired();
        friend.Property(x => x.Email).HasMaxLength(254);
        friend.HasIndex(x => x.Name);
        var loan = model.Entity<Loan>();
        loan.ToTable("loans", t => t.HasCheckConstraint("ck_return_date", "returned_at IS NULL OR returned_at >= loaned_at"));
        loan.HasKey(x => x.Id); loan.Ignore(x => x.IsActive);
        loan.HasOne<Game>().WithMany().HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Restrict);
        loan.HasOne<Friend>().WithMany().HasForeignKey(x => x.FriendId).OnDelete(DeleteBehavior.Restrict);
        loan.HasIndex(x => x.GameId).IsUnique().HasDatabaseName("ux_loans_active_game").HasFilter("returned_at IS NULL");
        loan.HasIndex(x => new { x.FriendId, x.LoanedAt });
        loan.HasIndex(x => x.LoanedAt);
        var run = model.Entity<ImportRun>();
        run.ToTable("catalog_import_runs"); run.HasKey(x => x.Id);
        run.HasIndex(x => new { x.FileHash, x.ParserVersion }).HasFilter("status = 'Completed'");
        foreach (var entity in model.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
                property.SetColumnName(System.Text.RegularExpressions.Regex.Replace(property.Name, "(?<!^)([A-Z])", "_$1").ToLowerInvariant());
    }
}
