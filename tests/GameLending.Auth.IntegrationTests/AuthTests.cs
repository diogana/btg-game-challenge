using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameLending.Auth.Application;
using GameLending.Auth.Infrastructure;
using GameLending.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
namespace GameLending.Auth.IntegrationTests;

public sealed class AuthFactory : WebApplicationFactory<Program>
{
    public const string Key = "integration-test-only-signing-key-32-bytes";
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:SigningKey"] = Key }));
    }
}
public sealed class AuthTests(AuthFactory factory) : IClassFixture<AuthFactory>
{
    [Fact]
    public async Task TokenCanBeValidated()
    {
        using var client = factory.CreateClient(); var response = await client.PostAsJsonAsync("/api/v1/auth/token", new { clientId = "demo", clientSecret = "secret" });
        response.EnsureSuccessStatusCode(); var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token!.AccessToken);
        var validation = await client.GetAsync("/api/v1/auth/validate"); Assert.Equal(HttpStatusCode.OK, validation.StatusCode);
    }
    [Theory]
    [InlineData("")]
    [InlineData("broken.token.value")]
    public async Task MissingOrMalformedTokenReturns401(string token)
    {
        using var client = factory.CreateClient(); if (token.Length > 0) client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/auth/validate")).StatusCode);
    }
    [Theory]
    [InlineData("bad-key")]
    [InlineData("wrong-issuer")]
    [InlineData("wrong-audience")]
    [InlineData("expired")]
    public async Task InvalidSignedTokenReturns401(string kind)
    {
        var settings = new JwtSettings { SigningKey = kind == "bad-key" ? "another-integration-key-at-least-32-bytes" : AuthFactory.Key };
        if (kind == "wrong-issuer") settings.Issuer = "Other"; if (kind == "wrong-audience") settings.Audience = "Other";
        var clock = new FixedClock(kind == "expired" ? DateTimeOffset.UtcNow.AddHours(-2) : DateTimeOffset.UtcNow);
        using var client = factory.CreateClient(); client.DefaultRequestHeaders.Authorization = new("Bearer", new JwtTokenService(settings, clock).Create("demo").AccessToken);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/auth/validate")).StatusCode);
    }
    [Fact]
    public async Task WhitespaceCredentialsReturn400()
    { using var client = factory.CreateClient(); Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/auth/token", new { clientId = " ", clientSecret = "secret" })).StatusCode); }
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider { public override DateTimeOffset GetUtcNow() => now; }
}
