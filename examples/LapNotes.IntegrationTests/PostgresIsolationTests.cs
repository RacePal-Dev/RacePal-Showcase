using LapNotes.Api;
using Npgsql;
using Xunit;

namespace LapNotes.IntegrationTests;

public sealed class PostgresIsolationTests
{
    [Fact]
    public async Task Does_not_read_record_from_another_organization()
    {
        await using var database = await OpenDemoTestDatabase();
        var store = new PostgresLapNoteStore(database);
        var note = new LapNote(Guid.NewGuid(), "tenant_a", "Kart 12", 60_000, DateTimeOffset.UtcNow);

        await store.InsertAsync(note, CancellationToken.None);

        Assert.Null(await store.FindAsync(note.Id, "tenant_b", CancellationToken.None));
        var sameTenantNote = await store.FindAsync(note.Id, "tenant_a", CancellationToken.None);
        Assert.NotNull(sameTenantNote);
        Assert.Equal(note.Id, sameTenantNote.Id);
    }

    [Fact]
    public async Task Stores_sql_metacharacters_as_data_not_commands()
    {
        await using var database = await OpenDemoTestDatabase();
        var store = new PostgresLapNoteStore(database);
        const string maliciousLookingLabel = "Kart'; DROP TABLE demo_lap_notes;--";
        var note = new LapNote(Guid.NewGuid(), "tenant_a", maliciousLookingLabel, 59_000, DateTimeOffset.UtcNow);

        await store.InsertAsync(note, CancellationToken.None);

        var loaded = await store.FindAsync(note.Id, "tenant_a", CancellationToken.None);
        Assert.NotNull(loaded);
        Assert.Equal(maliciousLookingLabel, loaded.KartLabel);
    }

    private static async Task<NpgsqlDataSource> OpenDemoTestDatabase()
    {
        var connectionString = Environment.GetEnvironmentVariable("LAPNOTES_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Set LAPNOTES_TEST_POSTGRES for an isolated disposable PostgreSQL test database.");

        var database = NpgsqlDataSource.Create(connectionString);
        try
        {
            await using var connection = await database.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(
                await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "schema.sql")),
                connection);
            await command.ExecuteNonQueryAsync();
            return database;
        }
        catch
        {
            await database.DisposeAsync();
            throw;
        }
    }
}
