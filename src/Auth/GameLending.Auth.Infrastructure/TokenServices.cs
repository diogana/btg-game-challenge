using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameLending.Auth.Application;
using GameLending.Auth.Domain;
using GameLending.Security;
using Microsoft.IdentityModel.Tokens;
namespace GameLending.Auth.Infrastructure;

public sealed class ChallengeClientCredentialValidator : IClientCredentialValidator
{
    public Task<bool> Validate(ClientCredential credential, CancellationToken ct) => Task.FromResult(credential.IsWellFormed);
}
public sealed class JwtTokenService(JwtSettings settings, TimeProvider clock) : ITokenService
{
    public TokenResponse Create(string clientId)
    {
        var now = clock.GetUtcNow();
        var claims = new List<Claim>
        {
            new("sub",clientId),new("client_id",clientId),new("jti",Guid.NewGuid().ToString()),
            new("iat",now.ToUnixTimeSeconds().ToString(),ClaimValueTypes.Integer64),
            new("scope",string.Join(' ',Permissions.Roles.Keys))
        };
        claims.AddRange(Permissions.Roles.Values.Select(role => new Claim("roles", role)));
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims, now.UtcDateTime,
            now.AddMinutes(settings.ExpirationMinutes).UtcDateTime,
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)), SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", settings.ExpirationMinutes * 60);
    }
}
