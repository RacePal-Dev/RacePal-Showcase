namespace LapNotes.Api;

// Independently authored demonstration types; not RacePal proprietary contracts.
public sealed record CreateLapNoteRequest(string? KartLabel, int LapDurationMs);

public sealed record LapNote(
    Guid Id,
    string OrganizationId,
    string KartLabel,
    int LapDurationMs,
    DateTimeOffset RecordedAtUtc);

public sealed record LapNoteResponse(
    Guid Id,
    string KartLabel,
    int LapDurationMs,
    DateTimeOffset RecordedAtUtc)
{
    public static LapNoteResponse From(LapNote note) =>
        new(note.Id, note.KartLabel, note.LapDurationMs, note.RecordedAtUtc);
}
