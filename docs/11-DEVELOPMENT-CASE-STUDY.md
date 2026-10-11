# 11 — Development case study: from local telemetry to hosted processing

## Background

RacePal began with native Windows workflows and local telemetry processing. The architecture subsequently evolved toward hosted ingestion, analysis, and organization-scoped services while preserving validated local capabilities during the transition.

## Engineering constraints

Moving file import and computation entirely into a desktop client or long-running HTTP request creates challenges around resource limits, failure recovery, authorization, and independent verification. A large immediate rewrite would also introduce substantial regression and data-compatibility risks.

## Architecture strategy

The staged design separates desktop presentation, hosted API authority, durable Worker processing, persistence providers, and telemetry/analysis modules. Client and server responsibilities are explicit; protected hosted operations remain server-authoritative.

The implementation increments include:

- Establish module boundaries and allowed dependency directions.
- Introduce hosted API, Worker, and shared contracts alongside the native client.
- Build PostgreSQL-backed hosted persistence and organization-scoped authorization.
- Add durable ingestion jobs and versioned data artifacts.
- Expand deterministic analysis independently of the UI and HTTP entry points.
- Develop repeatable alpha deployment packaging and recovery-oriented validation.

## Delivery method

Changes are staged through architecture documents, bounded issues, pull requests, tests, and CI/security verification. Higher-risk decisions are gated by explicit approval. This allows existing behavior to remain testable while hosting boundaries and platform responsibilities evolve.

## Current outcome and next stages

The private codebase contains native and hosted components, a module-level test structure, security/verification workflows, and deployment support artifacts. The product remains pre-release: live operational validation and later simulation, optimization, or AI-oriented features are separate milestones. For capability-specific status, see [Delivery status](08-DELIVERY-STATUS.md).
