using GameLending.Auth.Application;
using GameLending.Auth.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GameLending.Auth.Api.Controllers;

[ApiController, Route("api/v1/auth")]
public sealed class AuthController(TokenIssuer issuer) : ControllerBase
{
    [HttpPost("token"), AllowAnonymous]
    [ProducesResponseType<TokenResponse>(200), ProducesResponseType<ProblemDetails>(400), ProducesResponseType<ProblemDetails>(401)]
    public async Task<IActionResult> Token(ClientCredential credential, CancellationToken ct)
    {
        if (!credential.IsWellFormed) return Problem(statusCode: 400, title: "Client ID and secret are required and must respect length limits.");
        var token = await issuer.Issue(credential, ct);
        return token is null ? Problem(statusCode: 401, title: "Invalid credentials") : Ok(token);
    }
    [HttpGet("validate"), Authorize]
    [ProducesResponseType<TokenValidationResponse>(200), ProducesResponseType<ProblemDetails>(401)]
    public IActionResult ValidateToken() => Ok(new TokenValidationResponse(true, User.FindFirst("client_id")?.Value,
        DateTimeOffset.FromUnixTimeSeconds(long.Parse(User.FindFirst("exp")!.Value)),
        User.FindAll("roles").Select(x => x.Value).ToArray(), User.FindFirst("scope")?.Value.Split(' ') ?? []));
}
public sealed record TokenValidationResponse(bool Valid, string? ClientId, DateTimeOffset Expiration, string[] Roles, string[] Scopes);
