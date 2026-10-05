using System.Net;
using System.Net.Http.Json;
using GameLending.Auth.Infrastructure;
using GameLending.Bff.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace GameLending.Bff.IntegrationTests;

public sealed class BffFactory : WebApplicationFactory<Program>
{
    public const string Key = "bff-integration-only-key-at-least-32-bytes";
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:SigningKey"] = Key }));
        builder.ConfigureServices(services => { services.RemoveAll<IBackendClient>(); services.AddSingleton<IBackendClient, StubBackend>(); });
    }
    private sealed class StubBackend : IBackendClient
    {
        public Task<T> Get<T>(string path, CancellationToken ct)
        {
            object value = path == "statistics" ? new Statistics(1, 1, 0, 1, 0) : new Page<LoanView>([], 0, 1, 10);
            return Task.FromResult((T)value);
        }
        public Task<T> Send<T>(HttpMethod method, string path, object? body, CancellationToken ct) => throw new NotSupportedException();
        public Task Delete(string path, CancellationToken ct) => throw new NotSupportedException();
    }
}
public sealed class BffHttpTests(BffFactory factory) : IClassFixture<BffFactory>
{
    [Fact]
    public async Task DashboardRequiresAuthentication()
    { using var client = factory.CreateClient(); Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/dashboard")).StatusCode); }
    [Fact]
    public async Task DashboardRequiresAllReadPermissions()
    { using var client = factory.CreateClient(); client.DefaultRequestHeaders.Authorization = new("Bearer", TestTokens.Create(BffFactory.Key, true, true)); Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/dashboard")).StatusCode); }
    [Fact]
    public async Task DashboardReturnsAggregatedPayload()
    {
        using var client = factory.CreateClient(); client.DefaultRequestHeaders.Authorization = new("Bearer", new JwtTokenService(new() { SigningKey = BffFactory.Key }, TimeProvider.System).Create("tests").AccessToken);
        var response = await client.GetFromJsonAsync<DashboardView>("/api/v1/dashboard"); Assert.Equal(1, response!.Statistics.AvailableGames);
    }
}
