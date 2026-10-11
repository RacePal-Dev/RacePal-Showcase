# Public showcase contribution rules

The private RacePal application is proprietary. This public repository is documentation plus a separately written, bounded technical sample. Do not reconstruct, copy, translate or mirror private RacePal code, algorithms, routes, client contracts, credentials or git history.

## Allowed changes

- Accurate architecture and product-status documentation, with links to public evidence.
- Synthetic screenshots and clearly fictional sample data.
- Independently authored teaching/reference code **only under `examples/`**, approved for publication and clearly labeled as separate from the private product.

No personal/customer data, secret tokens, signing keys, connection strings with live credentials, production identifiers or nonpublic deployments.

## Verification and review

1. Work on a branch and keep diffs scoped.
2. Run `python3 scripts/check_public_content.py` and `dotnet test examples/LapNotes.Tests/LapNotes.Tests.csproj -c Release` when applicable.
3. Require Gitleaks and public-content CI checks to pass; review source diffs and claims manually.
4. Submit a PR with evidence, known gaps, and any new publication risk. Do not merge without owner approval.
5. For changes to licensing, visibility, proprietary scope, or private-derived details, stop and obtain explicit authorization **before publishing even a draft branch**.

`main` is not an artifact distribution channel for the proprietary RacePal application.
