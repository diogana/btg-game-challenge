using System.IdentityModel.Tokens.Jwt;
using GameLending.Auth.Application;
using GameLending.Auth.Domain;
using GameLending.Auth.Infrastructure;
using GameLending.Security;
namespace GameLending.Auth.UnitTests;

public sealed class TokenTests
{
    private readonly JwtSettings settings = new() { SigningKey = "unit-test-key-32-bytes-long-never-production", ExpirationMinutes = 60 };
    [Fact]
    public void TokenContainsIdentityPermissionsAndExpiration()
    {
        var now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");
        var response = new JwtTokenService(settings, new FixedClock(now)).Create("client");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);
        Assert.Equal(settings.Issuer, jwt.Issuer); Assert.Contains(settings.Audience, jwt.Audiences);
        Assert.Equal("client", jwt.Subject); Assert.Equal("client", jwt.Claims.Single(x => x.Type == "client_id").Value);
        Assert.Equal(now.AddHours(1).UtcDateTime, jwt.ValidTo); Assert.Equal(6, jwt.Claims.Count(x => x.Type == "roles"));
        Assert.Contains("loans.write", jwt.Claims.Single(x => x.Type == "scope").Value);
        Assert.NotNull(jwt.Claims.Single(x => x.Type == "jti")); Assert.NotNull(jwt.Claims.Single(x => x.Type == "iat"));
    }
    [Theory]
    [InlineData("", "secret")]
    [InlineData("client", " ")]
    public async Task EmptyCredentialsAreRejected(string id, string secret)
    {
        var issuer = new TokenIssuer(new ChallengeClientCredentialValidator(), new JwtTokenService(settings, TimeProvider.System));
        await Assert.ThrowsAsync<ArgumentException>(() => issuer.Issue(new(id, secret), default));
    }
    [Fact] public async Task ChallengeAcceptsNonEmptyCredentials() => Assert.True(await new ChallengeClientCredentialValidator().Validate(new("any", "secret"), default));
    [Fact]
    public async Task AlternateValidatorCanRejectCredentials()
    { var issuer = new TokenIssuer(new RejectValidator(), new JwtTokenService(settings, TimeProvider.System)); Assert.Null(await issuer.Issue(new("client", "secret"), default)); }
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider { public override DateTimeOffset GetUtcNow() => now; }
    private sealed class RejectValidator : IClientCredentialValidator { public Task<bool> Validate(ClientCredential credential, CancellationToken ct) => Task.FromResult(false); }
}
