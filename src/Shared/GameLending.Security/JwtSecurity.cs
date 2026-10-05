using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
namespace GameLending.Security;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "GameLending.Auth";
    public string Audience { get; set; } = "GameLending";
    public string SigningKey { get; set; } = "";
    public int ExpirationMinutes { get; set; } = 60;
    public void Validate()
    {
        if (Encoding.UTF8.GetByteCount(SigningKey) < 32) throw new InvalidOperationException("JWT signing key must contain at least 32 bytes.");
        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience) || ExpirationMinutes is < 1 or > 1440)
            throw new InvalidOperationException("Invalid JWT configuration.");
    }
}
public static class Permissions
{
    public static readonly IReadOnlyDictionary<string, string> Roles = new Dictionary<string, string>
    {
        ["games.read"] = "GameReader",
        ["games.write"] = "GameWriter",
        ["friends.read"] = "FriendReader",
        ["friends.write"] = "FriendWriter",
        ["loans.read"] = "LoanReader",
        ["loans.write"] = "LoanWriter"
    };
}
public static class JwtSecurity
{
    public static IServiceCollection AddGameLendingSecurity(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<JwtSettings>(provider =>
        {
            var settings = provider.GetRequiredService<IConfiguration>().GetSection("Jwt").Get<JwtSettings>() ?? new();
            settings.Validate(); return settings;
        });
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme).Configure<JwtSettings>((options, settings) =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ClockSkew = TimeSpan.FromSeconds(5),
                RoleClaimType = "roles",
                NameClaimType = "client_id"
            };
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse(); context.Response.StatusCode = 401;
                    context.Response.Headers.WWWAuthenticate = "Bearer";
                    await Microsoft.AspNetCore.Http.HttpResponseJsonExtensions.WriteAsJsonAsync(context.Response,
                        new Microsoft.AspNetCore.Mvc.ProblemDetails { Status = 401, Title = "Authentication required" });
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = 403;
                    await Microsoft.AspNetCore.Http.HttpResponseJsonExtensions.WriteAsJsonAsync(context.Response,
                        new Microsoft.AspNetCore.Mvc.ProblemDetails { Status = 403, Title = "Permission denied" });
                }
            };
        });
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            foreach (var permission in Permissions.Roles)
                options.AddPolicy(permission.Key, policy => policy.RequireAuthenticatedUser().RequireRole(permission.Value)
                    .RequireAssertion(context => context.User.FindAll("scope").SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)).Contains(permission.Key)));
        });
        return services;
    }
}
