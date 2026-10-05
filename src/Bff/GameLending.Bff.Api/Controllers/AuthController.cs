using GameLending.Bff.Application;
using GameLending.Bff.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GameLending.Bff.Api.Controllers;

[ApiController, Route("api/v1/auth")]
public sealed class AuthController(IHttpClientFactory factory) : ControllerBase
{
    [HttpPost("token"), AllowAnonymous]
    public Task<TokenResponse> Token(LoginRequest input, CancellationToken ct) => new BackendClient(factory.CreateClient("auth")).Send<TokenResponse>(HttpMethod.Post, "auth/token", input, ct);
    [HttpGet("validate"), Authorize]
    public Task<TokenValidationResponse> ValidateToken(CancellationToken ct) => new BackendClient(factory.CreateClient("auth")).Get<TokenValidationResponse>("auth/validate", ct);
}
