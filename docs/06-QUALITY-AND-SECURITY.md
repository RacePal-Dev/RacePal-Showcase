# Quality, security and verification

## Private product

The privately developed RacePal project uses staged test and build gates, architecture checks, application-level authorization design and dependency review. These statements describe engineering practices, not proof that every deployment or recovery test is complete. See [Delivery status](08-DELIVERY-STATUS.md) for maturity.

## This public repository

CI includes:
- a public-content scan for disallowed files, image bounds, likely credentials and broken local links;
- .NET 10 build and xUnit unit tests for the **independently authored** [reference API](../examples/README.md);
- Gitleaks secret scanning across repository history.

Gitleaks and the baseline regex scan supplement manual review; neither proves that sensitive code, data or credentials cannot leak. Native GitHub secret scanning / push protection must be configured separately when available.

## Threat modeling and executable evidence

See the [short threat model](14-EXAMPLE-THREAT-MODEL.md) for authentication, tenant scoping, untrusted inputs, parameterized PostgreSQL access, secret handling, mitigations and residual risks. The [sample tests](../examples/LapNotes.Tests/ValidationTests.cs) verify validation and claim-format boundaries only; they do not exercise an actual PostgreSQL instance or an external identity provider.
