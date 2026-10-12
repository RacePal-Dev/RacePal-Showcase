# Security design and concise threat model

**Scope.** RacePal is a pre-release desktop/hosted platform. This page distinguishes high-level **private product architecture** from the independently authored [public API example](../examples/README.md). The example is not production-ready and does not reproduce proprietary source.

## Assets and trust boundaries

| Asset | Trust boundary | Control or design |
| --- | --- | --- |
| Organization-scoped records | Desktop → hosted API | Authenticate caller; authorize access per tenant on server |
| Customer telemetry and files | Untrusted files → parser/worker | Validate structure, lengths, sizes, bounds and parser resource budgets |
| PostgreSQL data | API → database | Parameterized SQL, constrained DB role, organization-scoped reads |
| Credentials and signing keys | Secret manager → runtime | Do not commit, restrict access, redact logs, rotate if disclosed |
| Processing results | Worker → persistent artifact | Durable job status, versioning and scoped retrieval |

## Authentication and authorization

The [standalone example API](../examples/LapNotes.Api/Program.cs) configures JWT bearer authentication against an external HTTPS issuer. ASP.NET Core's JWT middleware validates token signature, issuer, audience and lifetime. Data routes require authenticated callers; a trusted `org_id` claim is read from the validated token, not accepted from the client body or URL. SQL reads are scoped with `organization_id = @organization_id` and return 404 for unknown or out-of-scope identifiers.

This assumes the external issuer is trusted to assign organization claims correctly. The public sample does **not** implement an identity provider, token issuance, full production authorization, or the proprietary RacePal login process. The private product design describes OIDC and PKCE for relevant client authentication flows, but that architecture is not reproduced here.

## Input, persistence and secrets

- Range and length validation occurs before storage; PostgreSQL schema constraints provide defense in depth.
- Npgsql binds typed parameters instead of constructing SQL with string concatenation.
- The sample handles cancellation-aware async database I/O and excludes organization identifiers from response objects.
- Runtime connection strings and issuer/audience settings are supplied outside source control, through environment configuration or a dedicated secret manager.
- Deployment hardening still requires least-privileged database roles, TLS, CORS restrictions, rate limiting, request size limits, exception mapping, logging safeguards, and backup/access controls.
- Gitleaks checks the available Git history in CI; the public file validator adds basic pattern checks. Neither can guarantee a secret-free history. GitHub native secret scanning and push protection should also be enabled when available.

## Threats, controls and remaining risk

| Threat | Demonstrated / proposed control | Remaining work |
| --- | --- | --- |
| Forged/expired bearer token | JWT middleware with HTTPS authority, audience and signature/lifetime validation | Real identity provider and end-to-end auth tests |
| Broken object-level authorization (BOLA/IDOR) | Validated tenant claim and tenant-filtered SQL query | Ensure every production endpoint is scoped; role permissions |
| SQL injection | Npgsql parameters; PostgreSQL integration test with SQL metacharacters | Review future dynamic query shapes |
| Cross-tenant information exposure | PostgreSQL test proving a different org cannot retrieve another org's record | Tests for full HTTP surface and admin/service privileges |
| Oversized or hostile input | Input checks and PostgreSQL constraints | Operational request limits, flood protection |
| Leaked secret | Secret separation, manual review, CI history scan | Rotation, Git history remediation and native push protection |
| Dependency compromise | Pinned package versions and minimal workflow privileges | Patch cadence, alerts, provenance and dependency audits |
| Sensitive logs or errors | No private data in this example's responses | Generic error mapping and operational redaction |

## Evidence and limitation

Review the [API](../examples/LapNotes.Api/Program.cs), [PostgreSQL adapter](../examples/LapNotes.Api/PostgresLapNoteStore.cs), [schema](../examples/LapNotes.Api/schema.sql), [unit tests](../examples/LapNotes.Tests/ValidationTests.cs) and [PostgreSQL integration tests](../examples/LapNotes.IntegrationTests/PostgresIsolationTests.cs).

CI validates the sample build, unit tests and database behavior, but does **not** validate production deployment, actual identity-provider login, real telemetry ingestion, penetration resistance, or the private application's internal implementation.
