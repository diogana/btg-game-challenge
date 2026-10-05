using GameLending.Auth.Application;
using GameLending.Auth.Infrastructure;
using GameLending.Security;
using GameLending.ApiSupport;
var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsEnvironment("Challenge") && !builder.Environment.IsEnvironment("Testing"))
    throw new InvalidOperationException("The permissive credential validator requires Challenge or Testing environment.");
builder.Services.AddDocumentation("GameLending Auth");
builder.Services.AddGameLendingSecurity(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IClientCredentialValidator, ChallengeClientCredentialValidator>();
builder.Services.AddScoped<ITokenService, JwtTokenService>(); builder.Services.AddScoped<TokenIssuer>();
builder.Services.AddHealthChecks();
var app = builder.Build();
app.UseExceptionHandler(); app.UseDocumentation(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
app.MapHealthChecks("/health/live").AllowAnonymous(); app.MapHealthChecks("/health/ready").AllowAnonymous();
app.Run();
public partial class Program;
