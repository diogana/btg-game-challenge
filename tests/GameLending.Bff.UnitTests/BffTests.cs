using System.Net;
using System.Net.Http.Json;
using GameLending.Bff.Application;
using GameLending.Bff.Infrastructure;
using Microsoft.AspNetCore.Http;
namespace GameLending.Bff.UnitTests;

public sealed class BffTests
{
    [Fact]
    public async Task DashboardAggregatesThreeSources()
    {
        var backend = new StubBackend(); var result = await new ScreenQueries(backend).Dashboard(default);
        Assert.Equal(7, result.Statistics.TotalGames); Assert.Single(result.ActiveLoans); Assert.Equal(3, backend.Paths.Count);
    }
    [Fact]
    public async Task EmptyLibraryDoesNotFetchLoans()
    { var backend = new StubBackend(); var result = await new ScreenQueries(backend).Library("none", null, 1, 20, default); Assert.Empty(result.Games.Items); Assert.Single(backend.Paths); }
    [Fact]
    public async Task DownstreamFailureIsNotConvertedToPartialSuccess()
    { var backend = new StubBackend { Fail = true }; await Assert.ThrowsAsync<DownstreamException>(() => new ScreenQueries(backend).Dashboard(default)); }
    [Theory]
    [InlineData(404, 404)]
    [InlineData(500, 502)]
    public async Task MapsDownstreamStatus(int upstream, int expected)
    {
        using var http = new HttpClient(new StubHandler(_ => new HttpResponseMessage((HttpStatusCode)upstream) { Content = JsonContent.Create(new { title = "Failure" }) })) { BaseAddress = new("http://core/") };
        var error = await Assert.ThrowsAsync<DownstreamException>(() => new BackendClient(http).Get<object>("games", default)); Assert.Equal(expected, error.Status);
    }
    [Fact]
    public async Task PropagatesBearerPerRequestWithoutLeakingPreviousToken()
    {
        var context = new HttpContextAccessor { HttpContext = new DefaultHttpContext() }; context.HttpContext.Request.Headers.Authorization = "Bearer first";
        var headers = new List<string?>();
        using var http = new HttpClient(new BearerPropagationHandler(context) { InnerHandler = new StubHandler(request => { headers.Add(request.Headers.Authorization?.ToString()); return new(HttpStatusCode.OK); }) });
        await http.GetAsync("http://core/one"); context.HttpContext = new DefaultHttpContext(); await http.GetAsync("http://core/two");
        Assert.Equal("Bearer first", headers[0]); Assert.Null(headers[1]);
    }
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> action) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => Task.FromResult(action(request)); }
    private sealed class StubBackend : IBackendClient
    {
        public List<string> Paths = []; public bool Fail;
        public Task<T> Get<T>(string path, CancellationToken ct)
        {
            Paths.Add(path); if (Fail) throw new DownstreamException(503, "Unavailable");
            object result = path == "statistics" ? new Statistics(7, 6, 1, 2, 1) : path.StartsWith("games") ? new Page<GameView>([], 0, 1, 20) :
                new Page<LoanView>([new(Guid.NewGuid(), Guid.NewGuid(), "Game", Guid.NewGuid(), "Friend", DateTimeOffset.UtcNow, null)], 1, 1, 10);
            return Task.FromResult((T)result);
        }
        public Task<T> Send<T>(HttpMethod method, string path, object? body, CancellationToken ct) => throw new NotSupportedException();
        public Task Delete(string path, CancellationToken ct) => throw new NotSupportedException();
    }
}
