using System.Text;
using SyntheticTelemetry.Parser;
using Xunit;

namespace SyntheticTelemetry.Tests;

public sealed class ParserTests
{
    private const string Header = SyntheticTelemetryParser.Header;

    [Fact]
    public async Task Parses_valid_sample_with_invariant_decimal_values()
    {
        var trace = await Parse(Header + "\n0,38.5,8200\n1000,45.25,9500\n");
        Assert.Equal(2, trace.Samples.Count);
        Assert.Equal(new TelemetrySample(0, 38.5m, 8200), trace.Samples[0]);
        Assert.Equal(new TelemetrySample(1000, 45.25m, 9500), trace.Samples[1]);
    }

    [Fact]
    public async Task Accepts_crlf_and_single_final_newline()
    {
        var trace = await Parse(Header + "\r\n0,0,0\r\n");
        Assert.Single(trace.Samples);
    }

    [Fact]
    public async Task Accepts_exactly_maximum_rows()
    {
        var text = BuildRows(SyntheticTelemetryParser.MaxSampleRows);
        var trace = await Parse(text);
        Assert.Equal(SyntheticTelemetryParser.MaxSampleRows, trace.Samples.Count);
    }

    [Fact]
    public async Task Rejects_excess_rows()
    {
        var text = BuildRows(SyntheticTelemetryParser.MaxSampleRows + 1);
        await AssertRejected(text, TelemetryError.TooManySamples);
    }

    [Fact]
    public async Task Rejects_more_than_input_byte_budget_before_parsing()
    {
        byte[] source = new byte[SyntheticTelemetryParser.MaxInputBytes + 1];
        Array.Fill(source, (byte)'a');
        using var stream = new MemoryStream(source);
        var error = await Assert.ThrowsAsync<TelemetryParseException>(
            () => SyntheticTelemetryParser.ParseAsync(stream));
        Assert.Equal(TelemetryError.FileTooLarge, error.Reason);
    }

    [Fact]
    public async Task Rejects_invalid_utf8()
    {
        using var stream = new MemoryStream([0xFF, 0xFE, 0xFA]);
        var error = await Assert.ThrowsAsync<TelemetryParseException>(
            () => SyntheticTelemetryParser.ParseAsync(stream));
        Assert.Equal(TelemetryError.InvalidEncoding, error.Reason);
    }

    [Theory]
    [InlineData("")]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm")]
    public async Task Rejects_missing_samples(string data)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        var error = await Assert.ThrowsAsync<TelemetryParseException>(
            () => SyntheticTelemetryParser.ParseAsync(stream));
        Assert.Contains(error.Reason,
            new[] { TelemetryError.InvalidHeader, TelemetryError.MissingSamples });
    }

    [Theory]
    [InlineData("time,speed,rpm\n0,45,8500", TelemetryError.InvalidHeader)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n", TelemetryError.MissingSamples)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,20,2000\n\n", TelemetryError.InvalidRow)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,20,2000,extra", TelemetryError.InvalidRow)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,20", TelemetryError.InvalidRow)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,1e200,8000", TelemetryError.InvalidRow)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,40.5,8000\n0,41.0,8100", TelemetryError.NonMonotonicTimestamp)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n100,40.5,8000\n99,41.0,8100", TelemetryError.NonMonotonicTimestamp)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n3600001,40,9000", TelemetryError.ValueOutOfRange)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,181,9000", TelemetryError.ValueOutOfRange)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,-1,9000", TelemetryError.InvalidRow)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,40,18001", TelemetryError.ValueOutOfRange)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,40,-10", TelemetryError.InvalidRow)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,NaN,9000", TelemetryError.InvalidRow)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,40,9000\n10,40\t,9000", TelemetryError.InvalidCharacter)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,40,9000\n10,40,9000\rgarbage", TelemetryError.InvalidCharacter)]
    [InlineData("elapsed_ms,speed_kmh,engine_rpm\n0,40,9000\n10,40,9000\n\n", TelemetryError.InvalidRow)]
    public async Task Rejects_bad_csv_and_out_of_range_data(string text, TelemetryError reason) =>
        await AssertRejected(text, reason);

    [Fact]
    public async Task Rejects_unusually_long_line()
    {
        var text = Header + "\n0," + new string('9', SyntheticTelemetryParser.MaxLineLength) + ",1000";
        await AssertRejected(text, TelemetryError.LineTooLong);
    }

    [Fact]
    public async Task Rejects_non_ascii_characters()
    {
        await AssertRejected(Header + "\n0,4\u00E9,1000", TelemetryError.InvalidCharacter);
    }

    [Fact]
    public async Task Honors_pre_cancelled_token()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Header + "\n0,10,1000"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => SyntheticTelemetryParser.ParseAsync(stream, cts.Token));
    }

    [Fact]
    public async Task Parses_nonseekable_stream()
    {
        using var stream = new NonSeekableStream(Encoding.UTF8.GetBytes(Header + "\n0,10,1000"));
        var trace = await SyntheticTelemetryParser.ParseAsync(stream);
        Assert.Single(trace.Samples);
    }

    [Fact]
    public async Task Fuzzes_seeded_random_bytes_without_unexpected_exceptions()
    {
        var random = new Random(43811);
        for (int i = 0; i < 500; i++)
        {
            var payload = new byte[random.Next(1, 1024)];
            random.NextBytes(payload);
            using var input = new MemoryStream(payload);
            try
            {
                _ = await SyntheticTelemetryParser.ParseAsync(input);
            }
            catch (TelemetryParseException)
            {
                // Well-formed structured rejection is expected for arbitrary hostile bytes.
            }
        }
    }

    private static string BuildRows(int count)
    {
        var builder = new StringBuilder(Header);
        for (int i = 0; i < count; i++)
            builder.Append('\n').Append(i).Append(",40.25,8000");
        return builder.ToString();
    }

    private static async Task<TelemetryTrace> Parse(string source)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(source));
        return await SyntheticTelemetryParser.ParseAsync(stream);
    }

    private static async Task AssertRejected(string source, TelemetryError reason)
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(source));
        var thrown = await Assert.ThrowsAsync<TelemetryParseException>(
            () => SyntheticTelemetryParser.ParseAsync(input));
        Assert.Equal(reason, thrown.Reason);
    }

    private sealed class NonSeekableStream(byte[] data) : Stream
    {
        private readonly MemoryStream _inner = new(data);
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) =>
            _inner.Read(buffer, offset, count);
        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            _inner.ReadAsync(buffer, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();
        public override void SetLength(long value) =>
            throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing) _inner.Dispose();
            base.Dispose(disposing: disposing);
        }
    }
}
