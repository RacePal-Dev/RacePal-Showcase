# 04 — Engineering ownership and responsibilities

## Project leadership

RacePal's technical development is directed by its project owner, who is responsible for architecture, product and engineering planning, scope definition, integration, review, and technical acceptance. Software development is supported by AI coding tools operating within documented constraints and approval gates.

This combines **software architecture, desktop/backend engineering, and development lifecycle management**. It does not imply that every line of implementation was manually authored or that the project operates with a separately staffed enterprise organization.

## Responsibility areas

| Area | Engineering responsibilities |
| --- | --- |
| Product planning | Define product objectives, staged milestones, acceptance criteria, dependencies, and non-goals |
| Software architecture | Design module ownership, separation of concerns, client/server trust boundaries, and persistence responsibilities |
| Application development | Coordinate native client, API, Worker, telemetry and analysis workflows, and supporting integrations |
| Version control | Maintain Git branch strategy, scoped commits, pull requests, reviews, and documentation consistency |
| Quality engineering | Define validation requirements, investigate failed checks, track regressions, and enforce verification gates |
| Security | Review least-privilege boundaries, secrets handling, authorization, and dependency risks |
| Operations | Plan reproducible deployment packaging, backup/restore controls, migrations, and staged readiness checks |
| AI-assisted delivery | Define agent instructions, assign bounded tasks, inspect generated changes, and retain human approval over consequential decisions |

## Development accountability

Architectural decisions and significant changes are recorded before implementation. Execution is divided into bounded tasks, reviewed as code changes, and checked against explicit technical and security constraints. AI assistance supports research, drafting, implementation, and verification; it does not replace technical ownership or authorize deployments, security-policy changes, or unreviewed merges.

## Technical principles

- Keep domain and application behavior separate from UI and provider-specific infrastructure.
- Treat the installed desktop client as untrusted with respect to hosted authorization and protected operations.
- Prefer small, independently verifiable increments to large cross-cutting rewrites.
- Design failure handling, concurrency, data integrity, and recovery as architectural concerns.
- Preserve provenance, reproducibility, and versioning for analytical output.
- Use source control and automated validation as part of the delivery process, not as post-implementation add-ons.

Detailed account: [Software architecture and AI-assisted development](12-SOFTWARE-ARCHITECTURE-AND-AI-ASSISTED-DEVELOPMENT.md).

Related documents: [Architecture](02-ARCHITECTURE.md), [Engineering workflow](03-ENGINEERING-WORKFLOW.md), [Agentic development](05-AGENTS-AND-AUTOMATION.md), [Engineering guidebook](10-ENGINEERING-GUIDEBOOK.md).
