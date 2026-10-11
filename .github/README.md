# GitHub workflow and AI-assisted engineering

RacePal combines **human-led software architecture** with **AI-assisted development, structured reviews, and automated verification**. This directory shows the GitHub conventions and agent responsibilities behind that engineering workflow.

> **Public showcase:** The files here describe engineering practices. RacePal's proprietary application source, operational configuration, and private development history are not included.

## Engineering ownership

The project developer leads the technical direction: defining architecture and module boundaries, selecting technologies, planning delivery stages, coordinating implementation, and reviewing changes before integration. AI-assisted tooling supports specific tasks, while architectural decisions, security exceptions, and final approval remain human responsibilities.

## Development workflow

1. **Plan:** Define the objective, scope, acceptance criteria, technical dependencies, and risks.
2. **Design and delegate:** Establish component boundaries and assign bounded work to the appropriate developer or specialized agent.
3. **Implement:** Work on isolated Git branches with focused, reviewable changes.
4. **Verify:** Run applicable tests, static analysis, security checks, and documentation validation.
5. **Review and integrate:** Assess the diff, resolve findings, and merge through a pull request (PR) after required checks pass.

This is the engineering model documented for RacePal; the public repository runs **documentation checks only**, not the private application's build or deployment pipeline.

## Specialized AI agent roles

| Public agent profile | Responsibility | Human-control boundary |
| --- | --- | --- |
| [Architect / Planner](agents/racepal-architect.agent.md) | Breaks down approved documentation work and flags architecture or publication risks | Does not independently change architecture |
| [Implementer](agents/racepal-implementer.agent.md) | Makes narrowly scoped public documentation changes | No proprietary code, unauthorized merges, or deployments |
| [Test Engineer](agents/racepal-test-engineer.agent.md) | Verifies documentation coherence, links, and safety-check results | Cannot suppress or weaken failing checks |
| [Security Reviewer](agents/racepal-security-reviewer.agent.md) | Reviews confidentiality, secrets, private identifiers, and misleading claims | Read-only review; no unilateral risk acceptance |

These are **sanitized public role definitions**, not evidence of continuously running autonomous agents. Agent execution depends on separately configured tooling, permissions, and approved tasks.

## Repository instructions and quality gates

- [Agent operating rules](../AGENTS.md) define task boundaries, stop conditions, and approval requirements.
- [Copilot instructions](copilot-instructions.md) and [scoped instructions](instructions/) constrain documentation and security-sensitive changes.
- [Pull request template](pull_request_template.md) supports traceable reviews.
- [Public showcase safety checks](workflows/public-content-check.yml) run on pull requests and pushes to `main`. The [validation script](../scripts/check_public_content.py) checks for disallowed file types, common credential patterns, and broken relative Markdown links.
- The `main` branch is protected by pull-request and required-status-check rules. **A passing automated check is not a substitute for manual confidentiality and intellectual-property review.**

## Explore the wider architecture

See the [project overview](../README.md), [engineering workflow](../docs/03-ENGINEERING-WORKFLOW.md), [AI agents and automation](../docs/05-AGENTS-AND-AUTOMATION.md), and [software architecture & AI-assisted development](../docs/12-SOFTWARE-ARCHITECTURE-AND-AI-ASSISTED-DEVELOPMENT.md) for the full technical context.
