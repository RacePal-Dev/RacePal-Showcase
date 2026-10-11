# Security design and concise threat model

**Scope.** RacePal is a pre-release desktop/hosted platform. This page distinguishes high-level **private product architecture** from the independently authored [public API example](../examples/README.md). The example is not production-ready and does not reproduce proprietary source. No claim of deployed protection is made solely from this document.

## Assets and trust boundaries

| Asset | Trust boundary | Relevant control |
| --- | --- | --- |
| Organization-scoped sessions and observations | Desktop → hosted API | Authenticate caller; authorize resource access per tenant on the server |
| Customer telemetry and imported files | Untrusted files → parser/worker | Validate structure, lengths, ranges, sizes and parser resource budgets |
| PostgreSQL records | API/service → database | Parameterized SQL, constrained DB role, organization-scoped reads |
| Credentials, signing keys and API tokens | Secrets manager → runtime | Never commit; restrict access, redact logs, rotate after exposure |
| Processing results | Worker → persistent artifacts | Durable job state, versioning, integrity and authorization |

## Authentication and authorization

The [standalone example](../examples/LapNotes.Api/Program.cs) configures JWT bearer authentication using an external HTTPS identity provider. The API validates the token's issuer, audience, signature and lifetime via ASP.NET Core JWT middleware. Its data endpoints require authorization. An `org_id` claim is read from the *validated* token; the caller cannot select the organization through URL or JSON. Database reads include `organization_id = @organization_id` and return 404 for missing/out-of-scope resources.

**Caveat:** The example assumes the configured trusted issuer is authorized to assign `org_id` claims. This relationship must be verified for any real integration. No identity provider, authorization server, real token, or production role model is distributed here. RacePal's documented hosted design uses OpenID Connect (OIDC) and Proof Key for Code Exchange (PKCE) for its clients, alongside server-side authorization; this example is not its login client.

## Input, storage and secrets

- The example validates label length and duration bounds; its database schema repeats constraints for defense in depth.
- SQL values are bound via Npgsql parameters, not string concatenation.
- Tenant identifiers come from validated authentication claims. The service uses cancellation-aware asynchronous I/O.
- Connection strings and issuer/audience settings are supplied outside the repository via environment configuration or a managed secret store. No access credentials or signing keys are embedded.
- In deployed systems, use a least-privileged PostgreSQL role, TLS, access controls, operational rate limits, restricted CORS, telemetry-safe audit logs and secure backup retention.
- The showcase uses CI content checks and Gitleaks as automated checks. They do **not** guarantee that a repository or its history contains no secrets. GitHub native secret scanning/push protection, when available, must be enabled separately in repository security settings.

## Threat scenarios and residual risks

| Threat | Mitigation | Residual risk / further verification |
| --- | --- | --- |
| Forged or expired bearer token | HTTPS issuer metadata and JWT signature, issuer, audience and expiry validation | Identity-provider compromise, wrong trust configuration; integration tests required |
| Horizontal access across organizations (IDOR/BOLA) | Server-derived organization claim and tenant-scoped SQL predicate | Every future endpoint requires the same authorization pattern; integration coverage pending |
| SQL injection | Parameterized SQL commands and typed inputs | Validate all query shape changes; test storage layer against PostgreSQL |
| Malformed or oversized inputs | Input validation plus database checks | Rate limits and request-body size limits require deployment configuration |
| Secret accidentally committed | Secret separation, Gitleaks, reviewer checks, GitHub native protection if enabled | Scan results are imperfect; rotate affected credentials and handle Git history |
| Compromised dependency or CI supply chain | Dependency audit and read-only workflow permissions | Patch and pin dependencies/actions; review advisories and build provenance |
| Information leakage through logs/errors | Generic HTTP errors; avoid sensitive payload logging | Application-level exception handling and structured redaction still needed |

## Evidence and limits

- [Code](../examples/LapNotes.Api/Program.cs), [database adapter](../examples/LapNotes.Api/PostgresLapNoteStore.cs), [SQL schema](../examples/LapNotes.Api/schema.sql) and [unit tests](../examples/LapNotes.Tests/ValidationTests.cs) are directly inspectable.
- CI verifies compilation and unit tests for the reference project. These do **not** demonstrate PostgreSQL integration test coverage, configured external login, penetration testing, or deployed security controls.
- The private application's exact source, routes, deployment topology, algorithms and test environment remain undisclosed.
