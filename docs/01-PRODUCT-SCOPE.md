# 01 — Problem and scope

## Problem

Raw karting telemetry from vendor tools may require import, interpretation, and validation before drivers or teams can compare performance meaningfully. A useful analysis tool must retain data provenance, be robust to malformed inputs, and make repeatable comparisons without implying precision unsupported by source data.

## Product approach

A native Windows experience provides familiar workflows. A hosted .NET backend supplies authorization, persisted job state, and authority over hosted resources. Compute-heavy tasks run in a separate Worker instead of inside HTTP request handlers.

## Current capabilities (high level)

Local telemetry workflows, domain and persistence foundations, versioned file-processing artifacts, server-side API/Worker foundations, hosted ingestion/analysis workflows, and guarded security controls exist in the private implementation. Full maturity and production rollout are separate questions.

## Not claimed

This showcase does **not** claim: universal telemetry-device compatibility, production-wide cloud deployment, a finished AI coach, a complete simulation product, a public demo endpoint, or universally supported operating systems.
