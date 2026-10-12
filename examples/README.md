# Independently authored C#/.NET examples

These .NET 10 examples were **independently authored for the public showcase**. It is not extracted from RacePal's private implementation and does not reveal private endpoints, algorithms, application contracts, or infrastructure. It demonstrates transferable engineering techniques.

## Example 1 — LapNotes API

### Reviewable code

- [Minimal ASP.NET Core API](LapNotes.Api/Program.cs): external JWT bearer authentication, server-derived organization scope, validation and HTTP responses.
- [PostgreSQL adapter](LapNotes.Api/PostgresLapNoteStore.cs): asynchronous Npgsql commands, bound SQL parameters, and tenant-scoped reads.
- [SQL schema](LapNotes.Api/schema.sql): minimal fictional records and an input range constraint.
- [Unit tests](LapNotes.Tests/ValidationTests.cs): xUnit input-boundary and JSON response-shape tests.
- [HTTP security tests](LapNotes.Tests/HttpSecurityTests.cs): JWT validation, 401/403/404 responses, rejected conflicting organization claims, and tenant scoping.
- [PostgreSQL integration tests](LapNotes.IntegrationTests/PostgresIsolationTests.cs): actual database tests for cross-tenant isolation and SQL-injection-safe parameters.

### Run API tests

Install .NET 10 SDK. Unit tests need no database:

```powershell
dotnet test examples/LapNotes.Tests/LapNotes.Tests.csproj -c Release
```

For integration tests, provision a **disposable** local PostgreSQL database and configure `LAPNOTES_TEST_POSTGRES` in your environment to its connection string. Never target an operational database. Tests apply the example schema to that database.

```powershell
dotnet test examples/LapNotes.IntegrationTests/LapNotes.IntegrationTests.csproj -c Release
```

GitHub Actions provisions PostgreSQL 17 in an ephemeral service container and runs both test suites automatically.

### Run the API

Provision PostgreSQL separately, apply `LapNotes.Api/schema.sql`, and configure (outside this repository):

- `Auth__Authority`: trusted HTTPS OpenID Connect issuer;
- `Auth__Audience`: expected JWT audience;
- `ConnectionStrings__DemoDb`: PostgreSQL connection string, supplied via a local secret store or environment variable.

Then:

```powershell
dotnet run --project examples/LapNotes.Api/LapNotes.Api.csproj
```

A named authorization policy requires **exactly one** syntactically valid `org_id` claim. The app assumes the configured identity provider is trusted to issue organization claims.

`GET /health/live` is public. `POST /api/demo/lap-notes` and `GET /api/demo/lap-notes/{id}` require a valid token issued by the configured identity provider with a trusted `org_id` claim. No live database, client credentials, signing key, or real identity-provider configuration is included.

## Example 2 — Synthetic telemetry CSV parser

- [Parser source](SyntheticTelemetry.Parser/SyntheticTelemetryParser.cs) — UTF-8 validation, fixed byte/line/row limits, numeric range checks, monotonic timestamps and structured failure codes.
- [Adversarial tests](SyntheticTelemetry.Tests/ParserTests.cs) — oversized files, corrupted bytes, excessive rows, invalid input, nonseekable streams, cancellation and reproducible random-byte trials.
- [Format and limitations](SyntheticTelemetry.Parser/README.md) — an explicitly fictional format with **no** support for private RacePal, vendor, or ALFANO telemetry files.

```powershell
dotnet test examples/SyntheticTelemetry.Tests/SyntheticTelemetry.Tests.csproj -c Release
```

CI runs this parser suite separately from the API tests and PostgreSQL integration tests.

## Boundaries

- The API demo stores fictional, manually entered lap notes; the separate parser uses fabricated CSV inputs and is not integrated into the API or a production telemetry workflow.
- CI tests actual HTTP JWT validation with locally signed, disposable RSA tokens, plus isolated PostgreSQL queries. This is **not** proof of integration with a real identity provider, deployed tenant permissions, production operation or penetration resistance.
- A production service would also need migrations, operational rate limits, auditing, exception mapping, strict request limits and deployment controls.

See the [security threat model](../docs/14-EXAMPLE-THREAT-MODEL.md).
