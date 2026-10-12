using System.Globalization;
using System.Text;

namespace SyntheticTelemetry.Parser;

// Public reference format defined solely for this exercise; not a RacePal file format.
public readonly record struct TelemetrySample(long ElapsedMs, decimal SpeedKmh, int EngineRpm);

public sealed record TelemetryTrace(IReadOnlyList<TelemetrySample> Samples);

public enum TelemetryError
{
    FileTooLarge,
    InvalidEncoding,
    InvalidHeader,
    InvalidCharacter,
    LineTooLong,
    MissingSamples,
    TooManySamples,
    InvalidRow,
    ValueOutOfRange,
    NonMonotonicTimestamp
}

public sealed class TelemetryParseException(TelemetryError reason, int lineNumber)
    : FormatException($"Synthetic telemetry rejected: {reason}, line {lineNumber}.")
{
    public TelemetryError Reason { get; } = reason;
    public int LineNumber { get; } = lineNumber;
}

public static class SyntheticTelemetryParser
{
    public const int MaxInputBytes = 65_536;
    public const int MaxSampleRows = 512;
    public const int MaxLineLength = 96;
    public const long MaxElapsedMs = 3_600_000;
    public const string Header = "elapsed_ms,speed_kmh,engine_rpm";

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    /// <summary>
    /// Reads at most MaxInputBytes + 1 bytes, validates a completely fictional CSV
    /// structure and refuses invalid data. The fixed byte bound also caps the size
    /// of the intermediate buffer; no Stream.Length, Seek or trusted file name is used.
    /// This is bounded buffering, not an unbounded streaming parser.
    /// </summary>
    public static async Task<TelemetryTrace> ParseAsync(
        Stream input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.CanRead)
            throw new ArgumentException("The input stream must be readable.", nameof(input));

        using var buffer = new MemoryStream(capacity: MaxInputBytes);
        var chunk = new byte[4096];
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int remaining = MaxInputBytes + 1 - (int)buffer.Length;
            int count = await input.ReadAsync(
                chunk.AsMemory(0, Math.Min(chunk.Length, remaining)), cancellationToken);
            if (count == 0)
                break;

            if (buffer.Length + count > MaxInputBytes)
                throw new TelemetryParseException(TelemetryError.FileTooLarge, 0);

            buffer.Write(chunk, 0, count);
        }

        string text;
        try
        {
            text = StrictUtf8.GetString(buffer.GetBuffer().AsSpan(0, (int)buffer.Length));
        }
        catch (DecoderFallbackException)
        {
            throw new TelemetryParseException(TelemetryError.InvalidEncoding, 0);
        }

        return ParseBoundedText(text);
    }

    private static TelemetryTrace ParseBoundedText(string text)
    {
        var rows = new List<TelemetrySample>(capacity: Math.Min(MaxSampleRows, 32));
        int start = 0;
        int lineNumber = 0;
        long previousTimestamp = -1;

        while (start < text.Length)
        {
            int newLine = text.IndexOf('\n', start);
            int end = newLine >= 0 ? newLine : text.Length;
            var line = text.AsSpan(start, end - start);
            if (!line.IsEmpty && line[^1] == '\r')
                line = line[..^1];

            lineNumber++;
            if (line.Length > MaxLineLength)
                throw new TelemetryParseException(TelemetryError.LineTooLong, lineNumber);

            foreach (char character in line)
            {
                if (character > 127 || char.IsControl(character))
                    throw new TelemetryParseException(TelemetryError.InvalidCharacter, lineNumber);
            }

            if (lineNumber == 1)
            {
                if (!line.SequenceEqual(Header.AsSpan()))
                    throw new TelemetryParseException(TelemetryError.InvalidHeader, lineNumber);
            }
            else
            {
                if (rows.Count >= MaxSampleRows)
                    throw new TelemetryParseException(TelemetryError.TooManySamples, lineNumber);

                var sample = ParseRow(line, lineNumber);
                if (sample.ElapsedMs <= previousTimestamp)
                    throw new TelemetryParseException(TelemetryError.NonMonotonicTimestamp, lineNumber);

                rows.Add(sample);
                previousTimestamp = sample.ElapsedMs;
            }

            if (newLine < 0)
                break;
            start = newLine + 1;
        }

        if (lineNumber == 0)
            throw new TelemetryParseException(TelemetryError.InvalidHeader, 1);
        if (rows.Count == 0)
            throw new TelemetryParseException(TelemetryError.MissingSamples, lineNumber + 1);
        return new TelemetryTrace(rows.AsReadOnly());
    }

    private static TelemetrySample ParseRow(ReadOnlySpan<char> line, int lineNumber)
    {
        int first = line.IndexOf(',');
        if (first <= 0)
            throw new TelemetryParseException(TelemetryError.InvalidRow, lineNumber);
        int next = line[(first + 1)..].IndexOf(',');
        if (next <= 0)
            throw new TelemetryParseException(TelemetryError.InvalidRow, lineNumber);
        int second = first + 1 + next;
        if (second == line.Length - 1 || line[(second + 1)..].Contains(','))
            throw new TelemetryParseException(TelemetryError.InvalidRow, lineNumber);

        if (!long.TryParse(line[..first], NumberStyles.None, CultureInfo.InvariantCulture, out long elapsed)
            || !decimal.TryParse(line[(first + 1)..second], NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out decimal speed)
            || !int.TryParse(line[(second + 1)..], NumberStyles.None,
                CultureInfo.InvariantCulture, out int rpm))
            throw new TelemetryParseException(TelemetryError.InvalidRow, lineNumber);

        if (elapsed > MaxElapsedMs || speed is < 0 or > 180 || rpm is < 0 or > 18_000)
            throw new TelemetryParseException(TelemetryError.ValueOutOfRange, lineNumber);

        return new TelemetrySample(elapsed, speed, rpm);
    }
}
