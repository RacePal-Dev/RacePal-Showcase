# Synthetic telemetry parser — safe file input reference

This is an **independently authored educational example**, not a copy, translation, adaptation or description of the private RacePal ingestion logic. It does **not** accept real ALFANO exports or any vendor's file format.

## Fictional CSV layout

The only accepted header is `elapsed_ms,speed_kmh,engine_rpm`. Values must use invariant decimal syntax (a period, not a locale-specific comma). Exactly three unquoted comma-separated columns are required:

```csv
elapsed_ms,speed_kmh,engine_rpm
0,35.25,8200
250,38.5,8700
500,41.0,9100
```

| Field | Allowed values |
| --- | --- |
| `elapsed_ms` | Unsigned whole number 0–3,600,000; strictly increasing |
| `speed_kmh` | Decimal 0–180 inclusive; no exponent |
| `engine_rpm` | Unsigned whole number 0–18,000 |

Line endings may be LF or CRLF; one trailing newline is allowed. All other encodings, fields, blank rows, control characters, quoted CSV features, extra separators, and out-of-order rows are rejected.

## Defensive boundaries

- Up to **65,536 raw bytes**; the parser reads only one extra byte to detect over-limit streams and works without seeking.
- Up to **512 samples** and **96 characters per line**.
- Strict UTF-8 decoding and ASCII-only payload; malformed bytes are rejected.
- Bounded buffering: the input is read into a fixed-size bounded memory buffer before parsing, not processed as a fully streaming pipeline.
- Cancellable asynchronous reads. Note that a hostile or broken stream can still block until cancellation or I/O errors are handled by its caller.
- Clear error codes with line numbers; raw untrusted row contents are never included in parser exception messages.

Tests intentionally include malformed rows, invalid UTF-8, resource-cap violations, duplicate timestamps, cultural formatting mistakes, cancellation, nonseekable streams, and seeded random-byte inputs. Randomized tests are **not** a substitute for coverage-guided fuzzing or formal guarantees.

## Build and test

With .NET 10 SDK installed:

```powershell
dotnet test examples/SyntheticTelemetry.Tests/SyntheticTelemetry.Tests.csproj -c Release
```

For a standalone application, the caller can pass any readable `Stream` to `SyntheticTelemetryParser.ParseAsync(input, cancellationToken)`. The example deliberately omits upload endpoints, file paths, persistence, business analysis, and deployment.

The separate [LapNotes API example](../README.md) covers authentication, organization scoping and PostgreSQL. The [security threat model](../../docs/14-EXAMPLE-THREAT-MODEL.md) describes scope and remaining risks.
