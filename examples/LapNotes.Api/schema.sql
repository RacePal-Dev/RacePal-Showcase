-- Demonstration schema; intentionally not the proprietary RacePal data model.
CREATE TABLE IF NOT EXISTS demo_lap_notes
(
    id UUID PRIMARY KEY,
    organization_id VARCHAR(64) NOT NULL,
    kart_label VARCHAR(40) NOT NULL,
    lap_duration_ms INTEGER NOT NULL CHECK (lap_duration_ms BETWEEN 15000 AND 600000),
    recorded_at_utc TIMESTAMPTZ NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_demo_lap_notes_organization
    ON demo_lap_notes (organization_id);
