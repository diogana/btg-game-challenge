using GameLending.Core.Application;
using GameLending.Core.Infrastructure;
using GameLending.Core.Api;
using GameLending.Security;
using GameLending.ApiSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDocumentation("GameLending Core"); builder.Services.AddExceptionHandler<CoreExceptionHandler>();
builder.Services.AddGameLendingSecurity(builder.Configuration); builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<LendingDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Lending")));
builder.Services.AddScoped<IGameRepository, GameRepository>(); builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>(); builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICatalogQueries, CatalogQueries>(); builder.Services.AddScoped<FriendCommands>();
builder.Services.AddScoped<GameCommands>(); builder.Services.AddScoped<LoanCommands>(); builder.Services.AddScoped<CatalogImporter>();
builder.Services.AddHostedService<GameCatalogImportBackgroundService>();
builder.Services.AddHealthChecks().AddCheck<DatabaseReadiness>("postgres", tags: ["ready"]);
var app = builder.Build();
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<LendingDbContext>().Database.MigrateAsync(); return;
}
app.UseExceptionHandler(); app.UseDocumentation(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
app.MapHealthChecks("/health/live", new() { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new() { Predicate = x => x.Tags.Contains("ready") }).AllowAnonymous();
app.Run();
public partial class Program;
public sealed class DatabaseReadiness(IServiceScopeFactory scopes) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var scope = scopes.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<LendingDbContext>();
        try
        {
            await db.Games.AsNoTracking().Select(x => x.Id).Take(1).ToListAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch { return HealthCheckResult.Unhealthy("Database or schema unavailable"); }
    }
}
