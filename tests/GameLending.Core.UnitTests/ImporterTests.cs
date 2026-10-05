using System.Text;
using GameLending.Core.Infrastructure;
namespace GameLending.Core.UnitTests;

public sealed class ImporterTests
{
    [Fact]
    public async Task MapsRealObjectShapeAndMixedPrices()
    {
        const string json = """
        {"https://store.playstation.com/en-us/concept/1":{"title":"Game one","platforms":["PS5"],"genres":["Action"],"developer":"Studio","release_date":"7/21/2017","rating":4.32,"votes":10,"price":0.0},
         "https://store.playstation.com/en-us/concept/2":{"title":"Game two","platforms":[],"genres":[],"developer":"Studio","release_date":"bad","rating":0,"votes":0,"price":"Unavailable"}}
        """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var rows = new List<CatalogRecord>(); await foreach (var row in CatalogReader.Read(stream, default)) rows.Add(row);
        Assert.Equal(2, rows.Count); Assert.Equal(new DateOnly(2017, 7, 21), rows[0].Game!.ReleaseDate);
        Assert.Equal(0m, rows[0].Game!.SourcePrice); Assert.Null(rows[1].Game!.SourcePrice); Assert.Equal(2, rows[1].Warnings);
    }
    [Fact]
    public async Task RejectsBadEntryAndContinues()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""{"https://example.com/1":{"title":""},"https://example.com/2":{"title":"Valid"}}"""));
        var rows = new List<CatalogRecord>(); await foreach (var row in CatalogReader.Read(stream, default)) rows.Add(row);
        Assert.Null(rows[0].Game); Assert.Equal("Valid", rows[1].Game!.Title);
    }
    [Fact]
    public async Task RejectsArrayRoot()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("[]"));
        await Assert.ThrowsAsync<InvalidDataException>(async () => { await foreach (var _ in CatalogReader.Read(stream, default)) { } });
    }
    [Fact]
    public async Task SupportsCancellation()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}")); using var cancel = new CancellationTokenSource(); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => { await foreach (var _ in CatalogReader.Read(stream, cancel.Token)) { } });
    }
}
