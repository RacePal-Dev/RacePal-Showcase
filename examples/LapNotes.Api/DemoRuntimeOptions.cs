namespace LapNotes.Api;

// Example-only runtime settings; no credentials or tenant identifiers are committed.
public sealed class DemoRuntimeOptions
{
    public string? Authority { get; set; }
    public string? Audience { get; set; }
    public string? ConnectionString { get; set; }

    public bool IsConfigured()
    {
        return Uri.TryCreate(Authority, UriKind.Absolute, out var issuer)
            && issuer.Scheme == Uri.UriSchemeHttps
            && !string.IsNullOrWhiteSpace(Audience)
            && !string.IsNullOrWhiteSpace(ConnectionString);
    }
}
