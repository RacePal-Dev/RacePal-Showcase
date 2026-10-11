# 03 — Engineering workflow: design → implement → verify → review

## Architecture and planning

Development is structured around a product charter, architecture documents, Architecture Decision Records (ADRs), milestone plans, acceptance criteria, and explicit non-goals. Before an implementation increment, the affected modules, trust boundaries, failure modes, dependencies, and verification requirements are established.

## Bounded implementation

Milestones are divided into small, reviewable tasks with an objective, scope, risk classification, owner, dependencies, and tests. Material changes to security, stored-data compatibility, numerical correctness, deployment, or recurring costs require additional approval.

## Version control and review

The standard change path is:

```text
Approved task → dedicated Git branch → focused commits → pull request
→ targeted checks → CI/security checks → review → approved merge
```

Git and GitHub support traceability, diff review, parallel work isolation, and controlled integration. The private engineering repository contains PR templates, issue templates, architecture and ownership controls, pinned GitHub Actions, and review procedures.

These are practices aligned with professional software delivery. They are not a claim of third-party certification or a production enterprise deployment.

## Verification

Targeted tests run before broader suites where applicable. Build, format, integration, and architecture checks are used to identify regressions. A failing test is diagnosed rather than skipped or weakened to produce a passing CI result. Windows desktop checks and Linux hosted/PostgreSQL integration checks validate different runtime concerns.

## Operations and recovery

Local and hosted recovery workflows are documented separately. The private implementation includes backup-related tooling, guarded restoration controls, and alpha deployment validation. Their presence demonstrates recoverability engineering, not completion of live production recovery drills. See [Operations and recovery](09-OPERATIONS-AND-RECOVERY.md).

## Human approval

Automated coding tools work within approved scopes. Architecture changes, releases, security exceptions, repository administration, and production deployment remain subject to explicit human decisions. A passing automated check is one input to technical acceptance, not a substitute for it.
