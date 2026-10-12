using Npgsql;

namespace LapNotes.Api;

public sealed class PostgresLapNoteStore(NpgsqlDataSource dataSource) : ILapNoteStore
{
    public async Task InsertAsync(LapNote note, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO demo_lap_notes
                (id, organization_id, kart_label, lap_duration_ms, recorded_at_utc)
            VALUES
                (@id, @organization_id, @kart_label, @lap_duration_ms, @recorded_at_utc)
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", note.Id);
        command.Parameters.AddWithValue("organization_id", note.OrganizationId);
        command.Parameters.AddWithValue("kart_label", note.KartLabel);
        command.Parameters.AddWithValue("lap_duration_ms", note.LapDurationMs);
        command.Parameters.AddWithValue("recorded_at_utc", note.RecordedAtUtc);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<LapNote?> FindAsync(Guid id, string organizationId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, organization_id, kart_label, lap_duration_ms, recorded_at_utc
            FROM demo_lap_notes
            WHERE id = @id AND organization_id = @organization_id
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("organization_id", organizationId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new LapNote(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetFieldValue<DateTimeOffset>(4));
    }
}
