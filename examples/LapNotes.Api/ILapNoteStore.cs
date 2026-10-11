namespace LapNotes.Api;

public interface ILapNoteStore
{
    Task InsertAsync(LapNote note, CancellationToken cancellationToken);
    Task<LapNote?> FindAsync(Guid id, string organizationId, CancellationToken cancellationToken);
}
