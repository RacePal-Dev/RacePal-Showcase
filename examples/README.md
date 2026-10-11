# Standalone .NET example — LapNotes

This small .NET 10 reference project was **independently authored for the public showcase**. It is not extracted from RacePal's private implementation and does not reveal private endpoints, algorithms, application contracts, or infrastructure. It demonstrates transferable engineering techniques.

## Reviewable code

- [Minimal ASP.NET Core API](LapNotes.Api/Program.cs): external JWT bearer authentication, server-derived organization scope, validation and HTTP responses.
- [PostgreSQL adapter](LapNotes.Api/PostgresLapNoteStore.cs): asynchronous Npgsql commands, bound SQL parameters, and tenant-scoped reads.
- [SQL schema](LapNotes.Api/schema.sql): minimal fictional records and an input range constraint.
- [Unit tests](LapNotes.Tests/ValidationTests.cs): xUnit input-boundary and tenant-claim tests.
- [PostgreSQL integration tests](LapNotes.IntegrationTests/PostgresIsolationTests.cs): actual database tests for cross-tenant isolation and SQL-injection-safe parameters.

## Run tests

Install .NET 10 SDK. Unit tests need no database:

```powershell
dotnet test examples/LapNotes.Tests/LapNotes.Tests.csproj -c Release
```

For integration tests, provision a **disposable** local PostgreSQL database and configure `LAPNOTES_TEST_POSTGRES` in your environment to its connection string. Never target an operational database. Tests apply the example schema to that database.

```powershell
dotnet test examples/LapNotes.IntegrationTests/LapNotes.IntegrationTests.csproj -c Release
```

GitHub Actions provisions PostgreSQL 17 in an ephemeral service container and runs both test suites automatically.

## Run the API

Provision PostgreSQL separately, apply `LapNotes.Api/schema.sql`, and configure (outside this repository):

- `Auth__Authority`: trusted HTTPS OpenID Connect issuer;
- `Auth__Audience`: expected JWT audience;
- `ConnectionStrings__DemoDb`: PostgreSQL connection string, supplied via a local secret store or environment variable.

Then:

```powershell
dotnet run --project examples/LapNotes.Api/LapNotes.Api.csproj
```

`GET /health/live` is public. `POST /api/demo/lap-notes` and `GET /api/demo/lap-notes/{id}` require a valid token issued by the configured identity provider with a trusted `org_id` claim. No live database, client credentials, signing key, or real identity-provider configuration is included.

## Boundaries

- The sample is not telemetry ingestion; it stores fictional manually entered lap notes.
- CI tests database queries and validation but not end-to-end JWT issuance, API authorization with real tokens, deployment, performance or penetration testing.
- A production service would also need migrations, operational rate limits, auditing, exception mapping, strict request limits and deployment controls.

See the [security threat model](../docs/14-EXAMPLE-THREAT-MODEL.md).
