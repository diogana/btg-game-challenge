namespace GameLending.Bff.Application;

public sealed class ScreenQueries(IBackendClient core)
{
    public async Task<DashboardView> Dashboard(CancellationToken ct)
    {
        var stats = core.Get<Statistics>("statistics", ct);
        var active = core.Get<Page<LoanView>>("loans/active?pageSize=10", ct);
        var recent = core.Get<Page<LoanView>>("loans?pageSize=10", ct);
        await Task.WhenAll(stats, active, recent); return new(await stats, (await active).Items, (await recent).Items);
    }
    public async Task<LibraryView> Library(string? search, string? status, int page, int pageSize, CancellationToken ct)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var games = await core.Get<Page<GameView>>($"games?search={Uri.EscapeDataString(search ?? "")}&status={Uri.EscapeDataString(status ?? "")}&page={page}&pageSize={pageSize}", ct);
        if (games.Items.Count == 0) return new(games, []);
        var filter = string.Join('&', games.Items.Select(x => $"gameIds={x.Id}"));
        var loans = await core.Get<Page<LoanView>>($"loans/active?pageSize=100&{filter}", ct);
        return new(games, loans.Items);
    }
    public async Task<GameDetailsView> Game(Guid id, int page, CancellationToken ct)
    {
        var game = core.Get<GameView>($"games/{id}", ct);
        var history = core.Get<Page<LoanView>>($"loans?gameId={id}&page={page}", ct);
        await Task.WhenAll(game, history); return new(await game, await history);
    }
    public async Task<FriendSummaryView> Friend(Guid id, int page, CancellationToken ct)
    {
        var friend = core.Get<FriendView>($"friends/{id}", ct);
        var active = core.Get<Page<LoanView>>($"loans/active?friendId={id}&page={page}", ct);
        var history = core.Get<Page<LoanView>>($"loans?friendId={id}&page={page}", ct);
        await Task.WhenAll(friend, active, history); return new(await friend, await active, await history);
    }
}
