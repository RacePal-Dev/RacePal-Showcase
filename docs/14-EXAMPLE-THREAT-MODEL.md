# Security design and concise threat model

**Scope.** RacePal is a pre-release desktop/hosted platform. This page distinguishes high-level **private product architecture** from the independently authored [public API example](../examples/README.md). The example is not production-ready and does not reproduce proprietary source.

## Assets and trust boundaries

| Asset | Trust boundary | Control or design |
| --- | --- | --- |
| Organization-scoped records | Desktop → hosted API | Authenticate caller; authorize access per tenant on server |
| Customer telemetry and files | Untrusted files → parser/worker | Private ingestion is not disclosed; an [independent synthetic CSV example](../examples/SyntheticTelemetry.Parser/README.md) demonstrates bounded input and fail-closed parsing |
| PostgreSQL data | API → database | Parameterized SQL and organization-scoped reads; constrained DB role recommended but not implemented in the sample |
| Credentials and signing keys | Secret manager → runtime | Do not commit, restrict access, redact logs, rotate if disclosed |
| Processing results | Worker → persistent artifact | Durable job status, versioning and scoped retrieval |

## Authentication and authorization

The [standalone example API](../examples/LapNotes.Api/Program.cs) configures JWT bearer authentication against an external HTTPS issuer. ASP.NET Core's JWT middleware validates token signature, issuer, audience and lifetime. Data routes require authenticated callers; a trusted `org_id` claim is read from the validated token, not accepted from the client body or URL. SQL reads are scoped with `organization_id = @organization_id` and return 404 for unknown or out-of-scope identifiers. The route group's named authorization policy rejects missing, duplicate and malformed `org_id` claims.

This assumes the external issuer is trusted to assign organization claims correctly. The public sample does **not** implement an identity provider, full production authorization, or the proprietary RacePal login process. [HTTP tests](../examples/LapNotes.Tests/HttpSecurityTests.cs) use throwaway RSA signing keys and in-process issuer metadata to exercise JWT validation without contacting an external identity provider. The private product design describes OIDC and PKCE for relevant client authentication flows, but that architecture is not reproduced here.

## Independently authored file-parsing reference

The separate [synthetic parser](../examples/SyntheticTelemetry.Parser/README.md) operates on a fabricated CSV layout, unrelated to actual RacePal or vendor telemetry. It demonstrates a 64 KiB input budget, 512-row cap, bounded line length, strict UTF-8, numeric/range checks, cancellation support, timestamp ordering, and structured failure reporting that avoids reflecting attacker input. Unit tests include malformed and seeded randomized bytes. This is **not** an integration test of the private telemetry ingestion pipeline; it does not establish actual per-request upload limits or CPU-time quotas for a deployed service.

## Input, persistence and secrets

- Range and length validation occurs before storage; PostgreSQL schema constraints provide defense in depth.
- Npgsql binds typed parameters instead of constructing SQL with string concatenation.
- The sample handles cancellation-aware async database I/O and excludes organization identifiers from response objects.
- Runtime connection strings and issuer/audience settings are supplied outside source control, through environment configuration or a dedicated secret manager.
- Deployment hardening still requires least-privileged database roles, TLS, CORS restrictions, rate limiting, request size limits, exception mapping, logging safeguards, and backup/access controls. **Row-level security (RLS) is not implemented** and CI uses a disposable PostgreSQL superuser; tenant enforcement in this sample is application-side.
- Gitleaks checks the available Git history in CI; the public file validator adds basic pattern checks. Neither can guarantee a secret-free history. GitHub native secret scanning and push protection should also be enabled when available.

## Threats, controls and remaining risk

| Threat | Demonstrated / proposed control | Remaining work |
| --- | --- | --- |
| Forged/expired bearer token | JWT middleware plus local HTTP tests for expiry, signature algorithm, issuer and audience | Real identity-provider integration, revocation and production trust configuration |
| Broken object-level authorization (BOLA/IDOR) | Route-group authorization policy, strict single-org claim, tenant-filtered SQL and 403/404 HTTP tests | RLS/least-privilege role not included; production endpoints still need individual review |
| SQL injection | Npgsql parameters; PostgreSQL integration test with SQL metacharacters | Review future dynamic query shapes |
| Cross-tenant information exposure | PostgreSQL test proving a different org cannot retrieve another org's record | Tests for full HTTP surface and admin/service privileges |
| Oversized or hostile input | Input checks and PostgreSQL constraints | Operational request limits, flood protection |
| Leaked secret | Secret separation, manual review, CI history scan | Rotation, Git history remediation and native push protection |
| Dependency compromise | Pinned package versions and minimal workflow privileges | Patch cadence, alerts, provenance and dependency audits |
| Sensitive logs or errors | No private data in this example's responses | Generic error mapping and operational redaction |

## Evidence and limitation

Review the [API](../examples/LapNotes.Api/Program.cs), [PostgreSQL adapter](../examples/LapNotes.Api/PostgresLapNoteStore.cs), [schema](../examples/LapNotes.Api/schema.sql), [unit tests](../examples/LapNotes.Tests/ValidationTests.cs) and [PostgreSQL integration tests](../examples/LapNotes.IntegrationTests/PostgresIsolationTests.cs).

CI validates the sample build, local JWT authentication/authorization behavior at HTTP level, unit tests and database behavior. It does **not** validate production deployment, a real identity-provider login, deployed RLS, real telemetry ingestion, penetration resistance, or the private application's internal implementation.
