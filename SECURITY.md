# Security policy

This public repository provides architecture documentation, synthetic demo screenshots and a standalone reference API; the proprietary RacePal application is not distributed here.

To report a potential vulnerability, secret exposure or sensitive-data leak, contact the repository owner privately through GitHub rather than posting exploitable details in a public issue.

Do not include real telemetry, precise customer GPS traces, credentials, production URLs or internal logs in reports.

For reviewable example controls and explicitly documented gaps, see the [security design and threat model](docs/14-EXAMPLE-THREAT-MODEL.md). CI runs source tests and secret checks, but scanning does not guarantee the absence of secrets, including in commit history. Exposed credentials must be revoked or rotated immediately; deleting the latest file version is insufficient.
