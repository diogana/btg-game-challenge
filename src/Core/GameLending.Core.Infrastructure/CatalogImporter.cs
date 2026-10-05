using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using GameLending.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace GameLending.Core.Infrastructure;

public sealed record CatalogRecord(Game? Game, int Warnings, string? Error);
public static class CatalogReader
{
    public static async IAsyncEnumerable<CatalogRecord> Read(Stream stream, [EnumeratorCancellation] CancellationToken ct)
    {
        using var text = new StreamReader(stream, leaveOpen: true);
        using var reader = new JsonTextReader(text) { DateParseHandling = DateParseHandling.None };
        if (!await reader.ReadAsync(ct) || reader.TokenType != JsonToken.StartObject) throw new InvalidDataException("Dataset root must be an object keyed by URL.");
        var completed = false;
        while (await reader.ReadAsync(ct))
        {
            if (reader.TokenType == JsonToken.EndObject) { completed = true; break; }
            if (reader.TokenType != JsonToken.PropertyName) throw new InvalidDataException("Expected a catalog URL property.");
            var url = (string)reader.Value!;
            await reader.ReadAsync(ct);
            var value = await JToken.ReadFromAsync(reader, ct);
            CatalogRecord record;
            try { record = Map(url, value); }
            catch (Exception ex) when (ex is DomainException or FormatException or InvalidCastException or ArgumentException or OverflowException or JsonException or InvalidOperationException)
            { record = new(null, 0, "Invalid catalog entry"); }
            yield return record;
        }
        if (!completed) throw new InvalidDataException("Dataset object was not closed.");
        if (await reader.ReadAsync(ct)) throw new InvalidDataException("Unexpected trailing JSON content.");
    }
    private static CatalogRecord Map(string url, JToken value)
    {
        var game = Game.Create(value.Value<string>("title") ?? "", value["platforms"]?.Values<string>().Select(x => x ?? "").ToArray() ?? [], DateTimeOffset.UtcNow);
        var warnings = 0;
        DateOnly? release = null;
        if (DateOnly.TryParseExact(value.Value<string>("release_date"), "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) release = date;
        else warnings++;
        decimal? price = null;
        if (decimal.TryParse(value["price"]?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)) price = parsed;
        else warnings++;
        game.SetCatalogData(new(url), value["genres"]?.Values<string>().Select(x => x ?? "").ToArray() ?? [],
            value.Value<string>("developer"), release, value.Value<decimal?>("rating"), value.Value<long?>("votes"), price);
        return new(game, warnings, null);
    }
}
public sealed class CatalogImporter(LendingDbContext db, ILogger<CatalogImporter> logger)
{
    private const long LockId = 72501851;
    public async Task<ImportRun?> Import(string path, int batchSize, bool force, CancellationToken ct)
    {
        if (!File.Exists(path)) { logger.LogWarning("Catalog file is missing; import not performed"); return null; }
        await using var stream = File.OpenRead(path);
        var hash = Convert.ToHexString(await SHA256.HashDataAsync(stream, ct)); stream.Position = 0;
        await db.Database.OpenConnectionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_lock({LockId})", ct);
        try
        {
            if (!force && await db.ImportRuns.AnyAsync(x => x.FileHash == hash && x.ParserVersion == "1" && x.Status == "Completed", ct))
            { logger.LogInformation("Catalog already imported; file hash unchanged"); return null; }
            var run = new ImportRun { FileHash = hash, StartedAt = DateTimeOffset.UtcNow };
            db.ImportRuns.Add(run); await db.SaveChangesAsync(ct);
            logger.LogInformation("Catalog import started {RunId}", run.Id);
            try
            {
                var batch = new List<Game>();
                await foreach (var record in CatalogReader.Read(stream, ct))
                {
                    run.Analyzed++; run.Warnings += record.Warnings;
                    if (record.Game is null) run.Rejected++;
                    else batch.Add(record.Game);
                    if (batch.Count >= Math.Clamp(batchSize, 1, 2000)) await Flush(batch, run, ct);
                }
                await Flush(batch, run, ct);
                run.Status = "Completed"; run.FinishedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Catalog import finished {Analyzed} analyzed {Imported} imported {Duplicates} duplicates {Rejected} rejected {Warnings} warnings",
                    run.Analyzed, run.Imported, run.Duplicates, run.Rejected, run.Warnings);
                return run;
            }
            catch
            {
                foreach (var entry in db.ChangeTracker.Entries<Game>().ToArray()) entry.State = EntityState.Detached;
                run.Status = ct.IsCancellationRequested ? "Cancelled" : "Failed"; run.FinishedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(CancellationToken.None); throw;
            }
        }
        finally
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_unlock({LockId})", CancellationToken.None);
            await db.Database.CloseConnectionAsync();
        }
    }
    private async Task Flush(List<Game> batch, ImportRun run, CancellationToken ct)
    {
        if (batch.Count == 0) return;
        var urls = batch.Select(x => x.SourceUrl).ToArray();
        var known = (await db.Games.Where(x => urls.Contains(x.SourceUrl)).Select(x => x.SourceUrl!).ToListAsync(ct)).ToHashSet();
        foreach (var game in batch)
        {
            if (known.Add(game.SourceUrl!)) { db.Games.Add(game); run.Imported++; }
            else run.Duplicates++;
        }
        await db.SaveChangesAsync(ct);
        foreach (var entry in db.ChangeTracker.Entries<Game>().ToArray()) entry.State = EntityState.Detached;
        batch.Clear();
    }
}
public sealed class GameCatalogImportBackgroundService(IServiceScopeFactory scopes, IConfiguration config,
    ILogger<GameCatalogImportBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!config.GetValue("CatalogImport:Enabled", true)) return;
        try
        {
            using var scope = scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<CatalogImporter>().Import(
                config["CatalogImport:Path"] ?? "data/output.json", config.GetValue("CatalogImport:BatchSize", 500),
                config.GetValue("CatalogImport:Force", false), stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        catch (Exception ex) { logger.LogError("Catalog import failed ({ErrorType}); inspect dataset and database configuration", ex.GetType().Name); }
    }
}
