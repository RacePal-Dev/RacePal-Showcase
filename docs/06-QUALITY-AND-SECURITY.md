# 06 — Quality, security, and verification strategy

## Quality gates in the private repository

- Build/test/format verification for .NET components, including a Windows validation job.
- Linux-backed PostgreSQL integration testing for applicable hosted persistence behavior.
- Module/dependency direction and architecture-boundary verification.
- CodeQL (code scanning / static application security testing), dependency and supply-chain analysis.
- GitHub pull-request template requiring scope, risk level, verification evidence, and rollback implications.
- Repository guidance against changing or skipping tests merely to hide failures.

These controls are evidenced by configuration and engineering history, **not** by a claim that all checks are currently passing or that every release gate is complete.

## Threat-oriented design themes

| Risk | Architectural treatment |
| --- | --- |
| Untrusted local client | Hosted access decisions made server-side |
| Cross-organization access | Organization/tenant-scoped authorization and resource checks |
| Malformed imported files | Strict validation, bounds, defensive parsing |
| Incomplete background work | Durable job state, retry and recovery design |
| Data integrity | Versioned artifacts and deterministic result identifiers |
| Credential exposure | Secret separation, sanitized logs, no sample real tokens |
| Vulnerable dependencies | Central dependency management and automated review |

## Verification vocabulary

**Tested in repository** ≠ **deployed** ≠ **observed healthy in production**. This distinction is especially important for hosted operations, recovery drills, and environmental configurations.

## Security of this showcase

No private application code, connection strings, credentials, full API routes, proprietary numerical contracts, production telemetry, or raw deployment topology are reproduced. The public CI checks for some disallowed files and common credential patterns but cannot prove zero sensitive disclosures. A human publication review remains mandatory.
