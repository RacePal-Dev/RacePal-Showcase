# 10 — Engineering guidebook

## Purpose

RacePal uses a stable documentation hierarchy so engineering work is driven by product requirements, architecture decisions, and verifiable acceptance conditions rather than ad-hoc prompts or isolated code changes.

## Lifecycle

1. **Discover:** Establish the product problem, current behavior, assumptions, and constraints.
2. **Design:** Define module ownership, trust boundaries, data flows, failure handling, and trade-offs. Record material decisions in Architecture Decision Records (ADRs).
3. **Plan:** Divide changes into bounded increments with acceptance criteria, dependencies, risk, and non-goals.
4. **Implement:** Work on dedicated Git branches. AI assistants may contribute only within the approved task.
5. **Verify:** Run relevant tests, formatting/build checks, integration gates, and security scans. Investigate failed checks rather than bypassing them.
6. **Review:** Inspect diffs and validation evidence, update affected documentation, and resolve material issues.
7. **Integrate:** Merge once the applicable review and approval requirements are met.
8. **Operate and recover:** Validate migrations, deployment sequencing, logging, backup integrity, and restoration through separate operational checkpoints.

## Documentation layers

| Layer | Role |
| --- | --- |
| `PRODUCT.md` | Product boundaries, principles, and delivery vocabulary |
| System architecture and ADRs | Module responsibilities, invariants, decisions, and trade-offs |
| `PLANS.md` and milestone records | Delivery sequence and acceptance conditions |
| `AGENTS.md` and specialized agents | Instructions and decision gates for automated contributors |
| Issue and pull-request templates | Change tracking and review evidence |
| Verification scripts and CI | Reproducible automated checks |
| Security and operations guidance | Privacy, supply chain, deployment, and recovery controls |

## Governance principles

The workflow borrows established engineering practices such as design review, separation of concerns, change control, traceable version history, automated regression checks, and recovery planning. It does not represent a claim of compliance certification, audited controls, or production service-level commitments.

The core principle is simple: **define an engineering contract, implement within its boundaries, verify the result, and retain reviewable evidence of the change.**
