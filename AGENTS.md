# Agent instructions — public showcase

This repository contains **documentation and illustrative metadata only**. Agents must not attempt to regenerate or extract RacePal's proprietary source code.

## Order of authority

1. Human-approved task / issue and acceptance criteria.
2. This `AGENTS.md` and applicable `.github/instructions/` files.
3. `PRODUCT.md`, `docs/02-ARCHITECTURE.md`, and relevant public architecture decision records (ADRs).
4. Existing repository conventions and verification results.

## Workflow

1. Inspect the requested files, acceptance criteria, and relevant documentation.
2. Classify risk; define the smallest reviewable change and explicit non-goals.
3. Make documentation-only changes on a dedicated branch.
4. Run `python3 scripts/check_public_content.py` and review the diff.
5. Submit a pull request (PR) with validation evidence and limitations.
6. Wait for human review/merge; never merge, deploy, or change access/visibility on the agent's own authority.

## Stop conditions

Stop with `DECISION REQUIRED: <topic>` before changing licensing, repository visibility, product claims, security policy, scope, architecture descriptions, or publishing any content sourced from the private repository. State the options, trade-offs, and what needs approval.

## Publication constraints

- No application source (`.cs`, `.csproj`, `.sln`, `.xaml`), production algorithms, unredacted operational documentation, genuine customer telemetry, private GPS coordinates, secrets, credentials, keys, environment files, personal information, logs, backups, binaries, or private Git metadata.
- Never paste private source excerpts as examples. Synthetically authored text and test fixture samples must be labeled clearly.
- No external service or project status should be claimed unless supported by reviewed evidence.
- Distinguish implemented, packaged, planned, and deferred work.
- Do not claim automated agents run continuously merely because agent definitions exist.

## Definition of done

A PR must contain: purpose, exact files touched, verification command and output, claims review, privacy review, risks/limitations, and human approval of public-facing content. Green documentation CI is necessary but is **not sufficient** for release authorization.
