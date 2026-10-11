namespace LapNotes.Api;

public static class LapNoteValidator
{
    public static string? Validate(CreateLapNoteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.KartLabel))
            return "Kart label is required.";

        if (request.KartLabel.Trim().Length > 40)
            return "Kart label must be at most 40 characters.";

        if (request.LapDurationMs is < 15_000 or > 600_000)
            return "Lap duration must be between 15000 and 600000 milliseconds.";

        return null;
    }
}

public static class OrganizationScope
{
    // Only a trusted JWT's validated org_id claim is accepted, never a client body/query parameter.
    public static bool IsValid(string? value) =>
        value is { Length: >= 3 and <= 64 }
        && value.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_');
}
