using GameLending.Bff.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GameLending.Bff.Api.Controllers;

[ApiController, Route("api/v1")]
public sealed class OperationsController(IBackendClient core) : ControllerBase
{
    [HttpGet("friends"), Authorize(Policy = "friends.read")]
    public Task<Page<FriendView>> Friends(CancellationToken ct, string? search = null, int page = 1, int pageSize = 20) => core.Get<Page<FriendView>>($"friends?search={Uri.EscapeDataString(search ?? "")}&page={page}&pageSize={pageSize}", ct);
    [HttpGet("loans"), Authorize(Policy = "loans.read")]
    public Task<Page<LoanView>> Loans(CancellationToken ct, int page = 1, int pageSize = 20) => core.Get<Page<LoanView>>($"loans?page={page}&pageSize={pageSize}", ct);
    [HttpGet("loans/active"), Authorize(Policy = "loans.read")]
    public Task<Page<LoanView>> Active(CancellationToken ct, int page = 1, int pageSize = 20) => core.Get<Page<LoanView>>($"loans/active?page={page}&pageSize={pageSize}", ct);
    [HttpGet("loans/{id:guid}"), Authorize(Policy = "loans.read")]
    public Task<LoanView> Loan(Guid id, CancellationToken ct) => core.Get<LoanView>($"loans/{id}", ct);
    [HttpPost("games"), Authorize(Policy = "games.write"), ProducesResponseType<GameView>(201)]
    public async Task<IActionResult> CreateGame(SaveGame input, CancellationToken ct)
    { var item = await core.Send<GameView>(HttpMethod.Post, "games", input, ct); return StatusCode(201, item); }
    [HttpPut("games/{id:guid}"), Authorize(Policy = "games.write")]
    public Task<GameView> UpdateGame(Guid id, SaveGame input, CancellationToken ct) => core.Send<GameView>(HttpMethod.Put, $"games/{id}", input, ct);
    [HttpDelete("games/{id:guid}"), Authorize(Policy = "games.write"), ProducesResponseType(204)]
    public async Task<IActionResult> DeleteGame(Guid id, CancellationToken ct) { await core.Delete($"games/{id}", ct); return NoContent(); }
    [HttpPost("friends"), Authorize(Policy = "friends.write"), ProducesResponseType<FriendView>(201)]
    public async Task<IActionResult> CreateFriend(SaveFriend input, CancellationToken ct)
    { var item = await core.Send<FriendView>(HttpMethod.Post, "friends", input, ct); return StatusCode(201, item); }
    [HttpPut("friends/{id:guid}"), Authorize(Policy = "friends.write")]
    public Task<FriendView> UpdateFriend(Guid id, SaveFriend input, CancellationToken ct) => core.Send<FriendView>(HttpMethod.Put, $"friends/{id}", input, ct);
    [HttpDelete("friends/{id:guid}"), Authorize(Policy = "friends.write"), ProducesResponseType(204)]
    public async Task<IActionResult> DeleteFriend(Guid id, CancellationToken ct) { await core.Delete($"friends/{id}", ct); return NoContent(); }
    [HttpPost("loans"), Authorize(Policy = "loans.write"), ProducesResponseType<LoanView>(201)]
    public async Task<IActionResult> CreateLoan(CreateLoan input, CancellationToken ct) => StatusCode(201, await core.Send<LoanView>(HttpMethod.Post, "loans", input, ct));
    [HttpPost("loans/{id:guid}/return"), Authorize(Policy = "loans.write")]
    public Task<LoanView> ReturnLoan(Guid id, CancellationToken ct) => core.Send<LoanView>(HttpMethod.Post, $"loans/{id}/return", null, ct);
}
