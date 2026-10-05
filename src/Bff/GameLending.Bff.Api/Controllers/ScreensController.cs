using GameLending.Bff.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GameLending.Bff.Api.Controllers;

[ApiController, Route("api/v1")]
public sealed class ScreensController(ScreenQueries screens) : ControllerBase
{
    [HttpGet("dashboard"), Authorize(Policy = "games.read"), Authorize(Policy = "friends.read"), Authorize(Policy = "loans.read")]
    public Task<DashboardView> Dashboard(CancellationToken ct) => screens.Dashboard(ct);
    [HttpGet("library"), Authorize(Policy = "games.read"), Authorize(Policy = "loans.read")]
    public Task<LibraryView> Library(CancellationToken ct, string? search = null, string? status = null, int page = 1, int pageSize = 20) => screens.Library(search, status, page, pageSize, ct);
    [HttpGet("library/{id:guid}"), Authorize(Policy = "games.read"), Authorize(Policy = "loans.read")]
    public Task<GameDetailsView> Game(Guid id, CancellationToken ct, int page = 1) => screens.Game(id, page, ct);
    [HttpGet("friends/{id:guid}/summary"), Authorize(Policy = "friends.read"), Authorize(Policy = "loans.read")]
    public Task<FriendSummaryView> Friend(Guid id, CancellationToken ct, int page = 1) => screens.Friend(id, page, ct);
}
