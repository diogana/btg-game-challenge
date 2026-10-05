using GameLending.Core.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GameLending.Core.Api.Controllers;

[ApiController, Route("api/v1/loans")]
public sealed class LoansController(LoanCommands commands, ICatalogQueries queries) : ControllerBase
{
    [HttpGet, Authorize(Policy = "loans.read")]
    public Task<Page<LoanView>> List(CancellationToken ct, Guid? friendId = null, Guid? gameId = null, [FromQuery] Guid[]? gameIds = null, int page = 1, int pageSize = 20) => queries.Loans(false, friendId, gameId, gameIds, page, pageSize, ct);
    [HttpGet("active"), Authorize(Policy = "loans.read")]
    public Task<Page<LoanView>> Active(CancellationToken ct, Guid? friendId = null, Guid? gameId = null, [FromQuery] Guid[]? gameIds = null, int page = 1, int pageSize = 20) => queries.Loans(true, friendId, gameId, gameIds, page, pageSize, ct);
    [HttpGet("{id:guid}"), Authorize(Policy = "loans.read")]
    public async Task<LoanView> Get(Guid id, CancellationToken ct) => await queries.Loan(id, ct) ?? throw new NotFoundException("Loan");
    [HttpPost, Authorize(Policy = "loans.write"), ProducesResponseType<LoanView>(201)]
    public async Task<IActionResult> Create(CreateLoan input, CancellationToken ct)
    { var id = await commands.Create(input, ct); return CreatedAtAction(nameof(Get), new { id }, await queries.Loan(id, ct)); }
    [HttpPost("{id:guid}/return"), Authorize(Policy = "loans.write")]
    public async Task<LoanView> Return(Guid id, CancellationToken ct)
    { await commands.Return(id, ct); return (await queries.Loan(id, ct))!; }
}
[ApiController, Route("api/v1/statistics")]
public sealed class StatisticsController(ICatalogQueries queries) : ControllerBase
{
    [HttpGet, Authorize(Policy = "games.read"), Authorize(Policy = "friends.read"), Authorize(Policy = "loans.read")]
    public Task<Statistics> Get(CancellationToken ct) => queries.Statistics(ct);
}
