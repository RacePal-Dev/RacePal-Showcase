# 13 — Public documentation maintenance

## Purpose and scope

RacePal maintains a public technical showcase separately from the private application implementation. The showcase describes product capabilities, architectural boundaries, and engineering practices without distributing the proprietary source code.

**This is a documentation-publication process, not repository mirroring.** It does not synchronize source files, Git objects, test fixtures, internal plans, configuration, or histories between repositories.

## Change detection

A scheduled documentation review checks the private project's completed changes against the public descriptions. A proposed public update is useful when it materially changes one of the following:

- a **verified** capability or supported integration;
- the high-level responsibility or trust boundary of a module;
- the stated delivery status of a previously documented capability;
- an established engineering, security, quality, or operations practice;
- a publicly shareable demonstration screenshot or synthetic example.

Minor refactors, transient issues, internal implementation work, and unapproved plans do not automatically warrant public updates. An open pull request or a roadmap entry is not proof of delivered functionality.

## Publication boundary

Only independently authored, high-level descriptions may move into this repository.

**Permitted:** descriptive architecture diagrams, technology categories, concise capability summaries, release-stage clarifications, non-sensitive engineering practices, and synthetic or explicitly approved visual evidence.

**Excluded:** original application code, diffs, Git history, internal prompts or runbooks, proprietary numerical methods, unpublished interface or artifact contracts, private repository links or identifiers, operational infrastructure details, security-sensitive configurations, credentials, access tokens, customer information, private logs, databases, unredacted telemetry, and identifying GPS traces.

The publication decision is **fail closed**: if disclosure rights or confidentiality are unclear, no public branch, file, comment, or pull request is created. A public draft pull request is itself publicly visible.

## Controlled update procedure

1. Review completed implementation and authoritative documentation in the private project; check whether the public statement actually requires changing.
2. Classify each claim as **implemented**, **packaged/verified**, **planned**, **deferred**, or **deployed** based on the available evidence. Do not infer production operation from an alpha package or static validation.
3. Re-author a minimal public description rather than copying private text, code, file structures, exact contract details, or change logs.
4. Review the entire proposed public content for sensitive details **before writing anything to GitHub**. Where a claim needs private evidence, validate that evidence privately without exposing its link or identifier in the public artifact.
5. If the change is safe and substantive, prepare it on a dedicated public branch and open a **draft pull request**, avoiding duplicate PRs for the same update.
6. Run the repository's public-content check and required GitHub Actions status check; read the diff manually for confidentiality, correctness, scope, and status accuracy.
7. The project owner alone approves the publication and merges it into the protected `main` branch.

No automated reviewer may merge, bypass the branch ruleset, force-push the protected branch, grant publication permission, or modify the private application's production source.

## Automation and ownership

A scheduled AI-assisted review can identify documentation drift and prepare a sanitized draft proposal. The schedule is an external review task, **not a GitHub Action that clones the private repository**. GitHub Actions in this showcase validates public repository contents and is not evidence that a private-to-public export is safe on its own.

If no meaningful public-facing change exists, no proposal is required. If an update might expose private information, the automated process requests an owner's decision privately rather than opening a public draft.

Publication ownership and final approval remain human responsibilities.

## Corrections and security

Inaccurate status claims should be corrected with a focused documentation PR. Potentially sensitive disclosures must be treated as security incidents, not merely as formatting fixes. Stop publication, assess access and exposure, rotate any compromised credentials, and follow [SECURITY.md](../SECURITY.md). Closing a pull request or deleting a branch does not guarantee that public Git content has been erased.

## Related documentation

- [Delivery status](08-DELIVERY-STATUS.md)
- [Engineering workflow](03-ENGINEERING-WORKFLOW.md)
- [AI agents and automation](05-AGENTS-AND-AUTOMATION.md)
- [Quality and security](06-QUALITY-AND-SECURITY.md)
