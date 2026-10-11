# Deployment concepts — public only

Private project: alpha deployment packaging exists for separately hosted API, Worker, database, and artifact storage components with an HTTPS ingress concept. Database migrations, readiness, logging, backup and guarded restore are handled as explicit operations, not hidden first-start side effects.

This directory deliberately contains no Compose files, Caddy rules, image digests, environment variables, hostnames, internal addresses, certificates, backup scripts, or infrastructure details. **The public showcase cannot be deployed.**

See [operations and recovery](../docs/09-OPERATIONS-AND-RECOVERY.md) and [delivery status](../docs/08-DELIVERY-STATUS.md).
