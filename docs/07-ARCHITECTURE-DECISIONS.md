# 07 — Architecture decision records (public summaries)

The private repository maintains more extensive ADRs. These short summaries communicate **decision rationale** without copying internal acceptance thresholds or proprietary designs.

## Decision A: Modular monolith

**Context:** An early-stage multi-component application needs clear boundaries without the complexity of many independently deployed services.

**Decision:** Prefer a modular monolith with separate API/Worker processes where operationally valuable.

**Trade-off:** Easier integration, version consistency, and local development, but architecture discipline must prevent coupling.

## Decision B: Server authority and thin client

**Context:** Local binaries are inspectable and untrusted as a security boundary.

**Decision:** Hosted identity, permissions, organization ownership, and authoritative results belong on the server. Preserve a staged transition from local functionality.

**Trade-off:** Network and migration complexity in exchange for stronger centralized controls.

## Decision C: Durable background processing

**Context:** File import and analysis can outlast HTTP requests and need fault/retry handling.

**Decision:** Put durable work in a Worker; keep HTTP routes bounded and queryable.

**Trade-off:** Additional state machine and recovery behavior.

## Decision D: Versioned telemetry artifacts

**Context:** High-frequency samples need provenance, efficient storage, and reproducibility.

**Decision:** Store normalized/versioned artifacts and metadata deliberately rather than mapping every sample to a relational row.

**Trade-off:** Requires explicit artifact compatibility and retention management.

## Decision E: AI-assisted development with explicit gates

**Context:** Coding agents can accelerate repetitive engineering tasks but can also make changes outside intent.

**Decision:** Scope tasks tightly, require tests and PR evidence, and reserve high-risk decisions for humans.

**Trade-off:** Less autonomy, greater review predictability and auditability.
