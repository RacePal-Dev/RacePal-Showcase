using System.Security.Claims;

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
    public static bool IsValid(string? value) =>
        value is { Length: >= 3 and <= 64 }
        && value.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_');

    // Reject duplicate org_id claims instead of silently trusting FindFirst().
    public static bool TryGetSingle(ClaimsPrincipal? principal, out string? organizationId)
    {
        organizationId = null;
        var claims = principal?.FindAll("org_id").Take(2).ToArray();
        if (claims is not { Length: 1 } || !IsValid(claims[0].Value))
            return false;

        organizationId = claims[0].Value;
        return true;
    }
}
