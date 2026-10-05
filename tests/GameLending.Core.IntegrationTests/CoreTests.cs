using System.Net;
using System.Net.Http.Json;
using GameLending.Core.Application;
using GameLending.Core.Infrastructure;
using GameLending.Auth.Infrastructure;
using GameLending.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Testcontainers.PostgreSql;
namespace GameLending.Core.IntegrationTests;

public sealed class CoreFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? container;
    public string ConnectionString { get; private set; } = "";
    public const string Key = "core-integration-test-only-signing-key-32-bytes";
    public async Task InitializeAsync()
    {
        ConnectionString = Environment.GetEnvironmentVariable("TEST_POSTGRES_CONNECTION") ?? "";
        if (ConnectionString.Length == 0)
        {
            container = new PostgreSqlBuilder("postgres:17.6-bookworm").Build();
            await container.StartAsync(); ConnectionString = container.GetConnectionString();
        }
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<LendingDbContext>().Database.MigrateAsync();
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        { ["ConnectionStrings:Lending"] = ConnectionString, ["Jwt:SigningKey"] = Key, ["CatalogImport:Enabled"] = "false" }));
    }
    public HttpClient AuthenticatedClient()
    {
        var client = CreateClient(); client.DefaultRequestHeaders.Authorization = new("Bearer", new JwtTokenService(new() { SigningKey = Key }, TimeProvider.System).Create("tests").AccessToken); return client;
    }
    async Task IAsyncLifetime.DisposeAsync() { await DisposeAsync(); if (container is not null) await container.DisposeAsync(); }
}
public sealed class CoreTests(CoreFactory factory) : IClassFixture<CoreFactory>
{
    private async Task<(FriendView Friend, GameView Game)> Seed(HttpClient client)
    {
        var friend = await client.PostAsJsonAsync("/api/v1/friends", new SaveFriend("Test " + Guid.NewGuid(), null)); friend.EnsureSuccessStatusCode();
        var game = await client.PostAsJsonAsync("/api/v1/games", new SaveGame("Test " + Guid.NewGuid(), ["PS5"])); game.EnsureSuccessStatusCode();
        return ((await friend.Content.ReadFromJsonAsync<FriendView>())!, (await game.Content.ReadFromJsonAsync<GameView>())!);
    }
    [Fact]
    public async Task LoanReturnAndHistoryArePersisted()
    {
        using var client = factory.AuthenticatedClient(); var data = await Seed(client);
        var response = await client.PostAsJsonAsync("/api/v1/loans", new CreateLoan(data.Game.Id, data.Friend.Id)); Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var loan = (await response.Content.ReadFromJsonAsync<LoanView>())!;
        var game = await client.GetFromJsonAsync<GameView>($"/api/v1/games/{data.Game.Id}"); Assert.Equal("Borrowed", game!.Status); Assert.Equal(data.Friend.Name, game.FriendName);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/v1/friends/{data.Friend.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/v1/games/{data.Game.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/v1/loans/{loan.Id}/return", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync($"/api/v1/loans/{loan.Id}/return", null)).StatusCode);
        game = await client.GetFromJsonAsync<GameView>($"/api/v1/games/{data.Game.Id}"); Assert.Equal("Available", game!.Status);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/v1/games/{data.Game.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/v1/friends/{data.Friend.Id}")).StatusCode);
        var history = await client.GetFromJsonAsync<LoanView>($"/api/v1/loans/{loan.Id}"); Assert.Equal(data.Friend.Name, history!.FriendName); Assert.NotNull(history.ReturnedAt);
    }
    [Fact, Trait("Category", "Concurrency")]
    public async Task ConcurrentLoansAllowExactlyOneWinner()
    {
        using var first = factory.AuthenticatedClient(); using var second = factory.AuthenticatedClient(); var data = await Seed(first);
        var input = new CreateLoan(data.Game.Id, data.Friend.Id);
        var responses = await Task.WhenAll(first.PostAsJsonAsync("/api/v1/loans", input), second.PostAsJsonAsync("/api/v1/loans", input));
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Created); Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Conflict);
    }
    [Fact]
    public async Task DatabaseRejectsSecondActiveLoanDirectly()
    {
        using var client = factory.AuthenticatedClient(); var data = await Seed(client);
        (await client.PostAsJsonAsync("/api/v1/loans", new CreateLoan(data.Game.Id, data.Friend.Id))).EnsureSuccessStatusCode();
        await using var connection = new NpgsqlConnection(factory.ConnectionString); await connection.OpenAsync();
        await using var command = new NpgsqlCommand("INSERT INTO loans(id,game_id,friend_id,loaned_at) VALUES(@id,@game,@friend,@now)", connection);
        command.Parameters.AddWithValue("id", Guid.NewGuid()); command.Parameters.AddWithValue("game", data.Game.Id); command.Parameters.AddWithValue("friend", data.Friend.Id); command.Parameters.AddWithValue("now", DateTimeOffset.UtcNow);
        var error = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync()); Assert.Equal("23505", error.SqlState);
    }
    [Fact]
    public async Task MissingTokenReturns401()
    { using var client = factory.CreateClient(); Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/games")).StatusCode); }
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task RequiresBothRoleAndScope(bool role, bool scope)
    {
        using var client = factory.CreateClient(); client.DefaultRequestHeaders.Authorization = new("Bearer", TestTokens.Create(CoreFactory.Key, role, scope));
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/games")).StatusCode);
    }
    [Fact]
    public async Task UnknownGameReturns404()
    { using var client = factory.AuthenticatedClient(); Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/games/{Guid.NewGuid()}")).StatusCode); }
    [Fact]
    public async Task InactiveFriendCannotBorrow()
    {
        using var client = factory.AuthenticatedClient(); var data = await Seed(client); (await client.DeleteAsync($"/api/v1/friends/{data.Friend.Id}")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/v1/loans", new CreateLoan(data.Game.Id, data.Friend.Id))).StatusCode);
    }
    [Fact]
    public async Task ImportTwiceDoesNotDuplicateAndPreservesManualEdits()
    {
        var title = "Import " + Guid.NewGuid(); var url = "https://example.com/" + Guid.NewGuid();
        var file = Path.GetTempFileName(); await File.WriteAllTextAsync(file, System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object> { [url] = new { title, platforms = new[] { "PS5" }, genres = new[] { "Action" }, developer = "Studio", release_date = "7/21/2017", rating = 4.0, votes = 1, price = 0 } }));
        try
        {
            using var scope = factory.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<LendingDbContext>();
            var importer = new CatalogImporter(db, NullLogger<CatalogImporter>.Instance);
            var first = await importer.Import(file, 1, false, default); Assert.Equal(1, first!.Imported);
            var game = await db.Games.SingleAsync(x => x.SourceUrl == url); game.Update("Manual title", [], DateTimeOffset.UtcNow); await db.SaveChangesAsync();
            var second = await importer.Import(file, 1, true, default); Assert.Equal(0, second!.Imported); Assert.Equal(1, second.Duplicates);
            Assert.Equal(1, await db.Games.CountAsync(x => x.SourceUrl == url)); Assert.Equal("Manual title", (await db.Games.SingleAsync(x => x.SourceUrl == url)).Title);
        }
        finally { File.Delete(file); }
    }
}
