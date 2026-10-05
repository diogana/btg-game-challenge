using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GameLending.Bff.Application;
using Microsoft.AspNetCore.Http;
namespace GameLending.Bff.Infrastructure;

public sealed class BearerPropagationHandler(IHttpContextAccessor context) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var bearer = context.HttpContext?.Request.Headers.Authorization.ToString();
        if (AuthenticationHeaderValue.TryParse(bearer, out var header) && header.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            request.Headers.Authorization = header;
        return base.SendAsync(request, ct);
    }
}
public sealed class BackendClient(HttpClient http) : IBackendClient
{
    public Task<T> Get<T>(string path, CancellationToken ct) => Send<T>(HttpMethod.Get, path, null, ct);
    public async Task<T> Send<T>(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var response = await Execute(method, path, body, ct);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct) ?? throw new DownstreamException(502, "Invalid upstream response.");
    }
    public async Task Delete(string path, CancellationToken ct)
    { using var response = await Execute(HttpMethod.Delete, path, null, ct); }
    private async Task<HttpResponseMessage> Execute(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, path);
            if (body is not null) request.Content = JsonContent.Create(body);
            var response = await http.SendAsync(request, ct);
            if (response.IsSuccessStatusCode) return response;
            var status = (int)response.StatusCode;
            var message = "Upstream request failed.";
            if (status < 500)
            {
                try
                {
                    var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
                    if (payload.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String) message = detail.GetString()!;
                    else if (payload.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String) message = title.GetString()!;
                }
                catch (JsonException) { }
            }
            response.Dispose(); throw new DownstreamException(status >= 500 ? 502 : status, message);
        }
        catch (HttpRequestException) { throw new DownstreamException(503, "Upstream service unavailable."); }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested) { throw new DownstreamException(504, "Upstream timeout."); }
    }
}
