using GameLending.Core.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GameLending.Core.Api.Controllers;

[ApiController, Route("api/v1/games")]
public sealed class GamesController(GameCommands commands, ICatalogQueries queries) : ControllerBase
{
    [HttpGet, Authorize(Policy = "games.read")]
    public Task<Page<GameView>> List(CancellationToken ct, string? search = null, string? status = null, int page = 1, int pageSize = 20, bool includeInactive = false) => queries.Games(search, status, page, pageSize, includeInactive, ct);
    [HttpGet("available"), Authorize(Policy = "games.read")]
    public Task<Page<GameView>> Available(CancellationToken ct, string? search = null, int page = 1, int pageSize = 20) => queries.Games(search, "Available", page, pageSize, false, ct);
    [HttpGet("{id:guid}"), Authorize(Policy = "games.read")]
    public async Task<GameView> Get(Guid id, CancellationToken ct) => await queries.Game(id, ct) ?? throw new NotFoundException("Game");
    [HttpPost, Authorize(Policy = "games.write"), ProducesResponseType<GameView>(201)]
    public async Task<IActionResult> Create(SaveGame input, CancellationToken ct)
    { var id = await commands.Create(input, ct); return CreatedAtAction(nameof(Get), new { id }, await queries.Game(id, ct)); }
    [HttpPut("{id:guid}"), Authorize(Policy = "games.write")]
    public async Task<GameView> Update(Guid id, SaveGame input, CancellationToken ct)
    { await commands.Update(id, input, ct); return (await queries.Game(id, ct))!; }
    [HttpDelete("{id:guid}"), Authorize(Policy = "games.write"), ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await commands.Delete(id, ct); return NoContent(); }
}
