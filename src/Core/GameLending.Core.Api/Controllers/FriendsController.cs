using GameLending.Core.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GameLending.Core.Api.Controllers;

[ApiController, Route("api/v1/friends")]
public sealed class FriendsController(FriendCommands commands, ICatalogQueries queries) : ControllerBase
{
    [HttpGet, Authorize(Policy = "friends.read")]
    public Task<Page<FriendView>> List(CancellationToken ct, string? search = null, int page = 1, int pageSize = 20, bool includeInactive = false) => queries.Friends(search, page, pageSize, includeInactive, ct);
    [HttpGet("{id:guid}"), Authorize(Policy = "friends.read")]
    public async Task<FriendView> Get(Guid id, CancellationToken ct) => await queries.Friend(id, ct) ?? throw new NotFoundException("Friend");
    [HttpPost, Authorize(Policy = "friends.write"), ProducesResponseType<FriendView>(201)]
    public async Task<IActionResult> Create(SaveFriend input, CancellationToken ct)
    { var id = await commands.Create(input, ct); return CreatedAtAction(nameof(Get), new { id }, await queries.Friend(id, ct)); }
    [HttpPut("{id:guid}"), Authorize(Policy = "friends.write")]
    public async Task<FriendView> Update(Guid id, SaveFriend input, CancellationToken ct)
    { await commands.Update(id, input, ct); return (await queries.Friend(id, ct))!; }
    [HttpDelete("{id:guid}"), Authorize(Policy = "friends.write"), ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await commands.Delete(id, ct); return NoContent(); }
}
