# Quality, security and verification

## Private product

RacePal's private engineering project includes staged build/test gates, architecture checks, server-side authorization design and dependency review. These are documented practices, not a claim that every release or live deployment is complete. See [Delivery status](08-DELIVERY-STATUS.md).

## This public repository

CI runs:
- public-content validation for disallowed files, bounded screenshots, obvious credentials and broken links;
- .NET 10 compilation, xUnit input/HTTP/JWT security tests for the independent [reference API](../examples/README.md);
- PostgreSQL 17 integration tests exercising organization isolation and SQL parameterization with a disposable database;
- Gitleaks scanning the available Git history for likely secrets.

CI scan results supplement, not replace, manual source, license, privacy and history review. GitHub native secret scanning and push protection require separate repository settings and are not claimed as enabled.

## Threat modeling

See [Security design and threat model](14-EXAMPLE-THREAT-MODEL.md) for JWT/OIDC assumptions, tenant scoping, input validation, PostgreSQL access, secrets and residual risks. The HTTP tests validate JWTs signed by disposable in-process test keys, not by an operational OIDC provider. PostgreSQL tests verify application-side scoping but do not exercise row-level security (RLS) or an actual least-privileged application role.
