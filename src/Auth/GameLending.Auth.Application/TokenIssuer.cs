using GameLending.Auth.Domain;
namespace GameLending.Auth.Application;

public interface IClientCredentialValidator { Task<bool> Validate(ClientCredential credential, CancellationToken ct); }
public interface ITokenService { TokenResponse Create(string clientId); }
public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);
public sealed class TokenIssuer(IClientCredentialValidator validator, ITokenService tokens)
{
    public async Task<TokenResponse?> Issue(ClientCredential credential, CancellationToken ct)
    {
        if (!credential.IsWellFormed) throw new ArgumentException("Client ID and secret must be present and within length limits.");
        return await validator.Validate(credential, ct) ? tokens.Create(credential.ClientId.Trim()) : null;
    }
}
