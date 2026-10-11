# Desktop UI walkthrough — October 2026

These screenshots document the **local WPF desktop** workflow in isolated **TEST STORAGE** with deliberately fictitious karting metadata. They show UI states observed during manual testing; they are **not** claims of a live hosted deployment, real telemetry ingestion, measured lap data, or a finished analysis experience.

The gallery is chronological. Some images intentionally capture intermediate empty states, pre-refresh counters, and an initial configuration snapshot later superseded by a corrected, clearly estimated v2. Those are useful as development evidence but should not be treated as polished product screens.

For two curated current-state images, return to [RacePal project README](../../../README.md).

## Walkthrough archive

1. [01-project-workspace-empty.png](01-project-workspace-empty.png) — Initial project workspace before metadata was added.
2. [02-add-session-prerequisites.png](02-add-session-prerequisites.png) — Metadata-only session form before the required entities were created.
3. [03-add-session-in-workspace.png](03-add-session-in-workspace.png) — Session form opened over the project workspace.
4. [04-add-driver-form.png](04-add-driver-form.png) — Driver creation dialog (empty state).
5. [05-driver-record.png](05-driver-record.png) — Driver profile after a successful save.
6. [06-add-kart-form.png](06-add-kart-form.png) — Kart editor before entering metadata.
7. [07-kart-metadata-editor.png](07-kart-metadata-editor.png) — Kart editor displaying chassis, class, manufacturer, and engine fields.
8. [08-add-circuit-form.png](08-add-circuit-form.png) — Circuit editor with optional geolocation fields.
9. [09-circuit-demo-data.png](09-circuit-demo-data.png) — Circuit editor with clearly fictional demonstration data.
10. [10-configuration-needs-refresh.png](10-configuration-needs-refresh.png) — Kart configuration view showing its pre-refresh empty state; it does not indicate a missing database record.
11. [11-saved-kart-detail.png](11-saved-kart-detail.png) — Previously saved kart opened for editing.
12. [12-configuration-evidence-editor.png](12-configuration-evidence-editor.png) — Versioned setup editor with data-provenance fields.
13. [13-optional-engineering-metrics.png](13-optional-engineering-metrics.png) — Optional limits and engineering parameters in the setup editor.
14. [14-initial-configuration-v1.png](14-initial-configuration-v1.png) — Initial v1 snapshot with exploratory provenance values; replaced by a clearly estimated v2 demo.
15. [15-session-editor-internal-ids-redacted.png](15-session-editor-internal-ids-redacted.png) — Session dialog after prerequisites were populated. Internal identifiers have been redacted.
16. [16-first-session-record.png](16-first-session-record.png) — First saved metadata-only session in the initial project.
17. [17-v2-before-refresh.png](17-v2-before-refresh.png) — Version 2 before refreshing the session usage counter (transient stale UI state).
18. [18-historical-v1-inspection.png](18-historical-v1-inspection.png) — Historical v1 snapshot inspected alongside v2, illustrating immutable versions.

## Disclosure and limitations

- Every identity, kart, circuit, configuration and session was created solely for this demonstration.
- The selected session stores metadata only; **it is not evidence of processed ALFANO telemetry**.
- The early v1 setup uses exploratory evidence labels. The subsequent v2 explicitly marks the synthetic parameters as `Estimated · Low`.
- One intermediate screenshot displays a cached usage count of zero before an explicit refresh showed the actual count of one.
- Machine-generated internal identifiers in the session editor screenshot have been redacted.
- The historical Phase 0 screenshot is maintained separately in the repository root screenshot directory.
