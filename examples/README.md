# Standalone .NET example — LapNotes

This is a small, original and **independently authored** demonstration for code review. It is **not extracted** from RacePal's proprietary private implementation and does not expose its internal endpoints, algorithms, domain contracts, or infrastructure. It illustrates transferable C#/.NET engineering techniques, not proof that the private RacePal server runs these exact files.

## What's inside

- [Minimal ASP.NET Core API](LapNotes.Api/Program.cs): JWT bearer authentication and authorization, request validation, claim-derived organization scoping, and HTTP 201/404/403 semantics.
- [PostgreSQL data access](LapNotes.Api/PostgresLapNoteStore.cs): asynchronous Npgsql commands with parameterized queries and organization-scoped reads.
- [Schema](LapNotes.Api/schema.sql): minimal fictional record table and range constraint.
- [Unit tests](LapNotes.Tests/ValidationTests.cs): xUnit tests for boundary values and organization-claim input.

## Run the checks

With .NET 10 SDK installed:

```powershell
dotnet test examples/LapNotes.Tests/LapNotes.Tests.csproj -c Release
```

To run the API, provision a local PostgreSQL database, apply `schema.sql`, and set these configuration values **outside this repository**:

- `Auth__Authority` — HTTPS OpenID Connect issuer URL;
- `Auth__Audience` — expected JWT audience;
- `ConnectionStrings__DemoDb` — PostgreSQL connection string supplied through environment variables or a secret manager.

Then:

```powershell
dotnet run --project examples/LapNotes.Api/LapNotes.Api.csproj
```

The `GET /health/live` route is public. All `/api/demo/lap-notes` routes require a valid JWT from the configured issuer, carrying a trusted `org_id` claim. No sample token, password, signing secret, identity-provider setup, or live database is included. A ready-to-run authenticated integration environment is intentionally **out of scope**.

## Limits

- The unit tests do not prove end-to-end JWT issuance, PostgreSQL integration, database availability, or authorization in a deployed service; those require separate integration tests.
- Real production APIs need central exception handling, rate limiting, observability, migrations, least-privileged database roles, and integration/load tests.
- These manually entered fictional lap notes are **not telemetry measurements**.

Read the [example threat model](../docs/14-EXAMPLE-THREAT-MODEL.md).
