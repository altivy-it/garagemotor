BEGIN;

DO $$
BEGIN
    CREATE ROLE garagemotor_runtime LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
EXCEPTION
    WHEN duplicate_object THEN NULL;
END
$$;

DO $$
BEGIN
    CREATE ROLE garagemotor_migrations LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
EXCEPTION
    WHEN duplicate_object THEN NULL;
END
$$;

ALTER ROLE garagemotor_runtime LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
ALTER ROLE garagemotor_migrations LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;

CREATE SCHEMA IF NOT EXISTS garagemotor AUTHORIZATION garagemotor_migrations;
DO $$
BEGIN
    EXECUTE format('GRANT CONNECT ON DATABASE %I TO garagemotor_runtime', current_database());
    EXECUTE format(
        'ALTER ROLE garagemotor_runtime IN DATABASE %I SET search_path = garagemotor, pg_catalog',
        current_database());
END
$$;
GRANT USAGE ON SCHEMA garagemotor TO garagemotor_runtime;
REVOKE ALL ON SCHEMA garagemotor FROM PUBLIC;

SET ROLE garagemotor_migrations;

CREATE TABLE IF NOT EXISTS garagemotor.users (
    user_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    email_normalized varchar(254) NOT NULL,
    display_name varchar(100) NOT NULL,
    password_hash text NOT NULL,
    state text NOT NULL DEFAULT 'active' CHECK (state IN ('active', 'disabled')),
    version bigint NOT NULL DEFAULT 1 CHECK (version > 0),
    created_utc timestamptz NOT NULL DEFAULT clock_timestamp(),
    changed_utc timestamptz NOT NULL DEFAULT clock_timestamp(),
    CONSTRAINT users_email_normalized_nonempty CHECK (char_length(email_normalized) BETWEEN 3 AND 254),
    CONSTRAINT users_display_name_nonempty CHECK (char_length(display_name) BETWEEN 1 AND 100)
);

CREATE UNIQUE INDEX IF NOT EXISTS users_email_normalized_uq
    ON garagemotor.users (lower(email_normalized));

CREATE TABLE IF NOT EXISTS garagemotor.sessions (
    session_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES garagemotor.users (user_id) ON DELETE CASCADE,
    token_hash bytea NOT NULL UNIQUE CHECK (octet_length(token_hash) = 32),
    created_utc timestamptz NOT NULL DEFAULT clock_timestamp(),
    expires_utc timestamptz NOT NULL,
    revoked_utc timestamptz,
    CONSTRAINT sessions_expiry_after_creation CHECK (expires_utc > created_utc),
    CONSTRAINT sessions_lifetime_max_12_hours CHECK (expires_utc <= created_utc + interval '12 hours')
);

CREATE INDEX IF NOT EXISTS sessions_user_active_idx
    ON garagemotor.sessions (user_id, expires_utc)
    WHERE revoked_utc IS NULL;

CREATE TABLE IF NOT EXISTS garagemotor.recovery_batches (
    batch_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES garagemotor.users (user_id) ON DELETE CASCADE,
    created_utc timestamptz NOT NULL DEFAULT clock_timestamp(),
    expires_utc timestamptz NOT NULL,
    revoked_utc timestamptz,
    CONSTRAINT recovery_batches_expiry_after_creation CHECK (expires_utc > created_utc),
    CONSTRAINT recovery_batches_lifetime_max_12_months CHECK (expires_utc <= created_utc + interval '12 months'),
    UNIQUE (batch_id, user_id)
);

CREATE UNIQUE INDEX IF NOT EXISTS recovery_batches_one_active_per_user_uq
    ON garagemotor.recovery_batches (user_id)
    WHERE revoked_utc IS NULL;

CREATE TABLE IF NOT EXISTS garagemotor.recovery_codes (
    recovery_code_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES garagemotor.users (user_id) ON DELETE CASCADE,
    batch_id uuid,
    kind text NOT NULL CHECK (kind IN ('smtp', 'offline')),
    code_hash bytea NOT NULL CHECK (octet_length(code_hash) = 32),
    created_utc timestamptz NOT NULL DEFAULT clock_timestamp(),
    expires_utc timestamptz NOT NULL,
    consumed_utc timestamptz,
    revoked_utc timestamptz,
    CONSTRAINT recovery_codes_expiry_after_creation CHECK (expires_utc > created_utc),
    CONSTRAINT recovery_codes_kind_batch_consistency CHECK (
        (kind = 'smtp' AND batch_id IS NULL AND expires_utc <= created_utc + interval '15 minutes') OR
        (kind = 'offline' AND batch_id IS NOT NULL)
    ),
    FOREIGN KEY (batch_id, user_id)
        REFERENCES garagemotor.recovery_batches (batch_id, user_id)
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS recovery_codes_hash_uq
    ON garagemotor.recovery_codes (code_hash);

CREATE INDEX IF NOT EXISTS recovery_codes_user_active_idx
    ON garagemotor.recovery_codes (user_id, kind, expires_utc)
    WHERE consumed_utc IS NULL AND revoked_utc IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS recovery_codes_one_active_smtp_per_user_uq
    ON garagemotor.recovery_codes (user_id)
    WHERE kind = 'smtp' AND consumed_utc IS NULL AND revoked_utc IS NULL;

CREATE TABLE IF NOT EXISTS garagemotor.recovery_attempts (
    attempt_id uuid NOT NULL DEFAULT gen_random_uuid(),
    account_key_hash bytea NOT NULL CHECK (octet_length(account_key_hash) = 32),
    origin_key_hash bytea NOT NULL CHECK (octet_length(origin_key_hash) = 32),
    occurred_utc timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (attempt_id, occurred_utc)
) PARTITION BY RANGE (occurred_utc);

CREATE TABLE IF NOT EXISTS garagemotor.recovery_attempts_default
    PARTITION OF garagemotor.recovery_attempts DEFAULT;

CREATE INDEX IF NOT EXISTS recovery_attempts_account_time_idx
    ON garagemotor.recovery_attempts (account_key_hash, occurred_utc DESC);

CREATE INDEX IF NOT EXISTS recovery_attempts_origin_time_idx
    ON garagemotor.recovery_attempts (origin_key_hash, occurred_utc DESC);

CREATE TABLE IF NOT EXISTS garagemotor.operation_receipts (
    operation_key uuid NOT NULL,
    created_utc timestamptz NOT NULL DEFAULT clock_timestamp(),
    actor_id uuid NOT NULL REFERENCES garagemotor.users (user_id) ON DELETE CASCADE,
    operation_name varchar(100) NOT NULL,
    scope_kind text NOT NULL CHECK (scope_kind IN ('actor', 'garage')),
    scope_id uuid NOT NULL,
    garage_id uuid,
    request_hash bytea NOT NULL CHECK (octet_length(request_hash) = 32),
    status text NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'succeeded')),
    resource_id uuid,
    response_status smallint,
    response_version bigint,
    response_etag varchar(128),
    response_payload jsonb,
    PRIMARY KEY (operation_key, created_utc),
    CONSTRAINT operation_receipts_scope_consistency CHECK (
        (scope_kind = 'actor' AND scope_id = actor_id) OR
        (scope_kind = 'garage' AND garage_id IS NOT NULL AND scope_id = garage_id)
    ),
    CONSTRAINT operation_receipts_response_consistency CHECK (
        (status = 'pending' AND resource_id IS NULL AND response_status IS NULL AND response_version IS NULL AND response_payload IS NULL) OR
        (status = 'succeeded' AND resource_id IS NOT NULL AND response_status IS NOT NULL AND response_status BETWEEN 200 AND 299 AND response_version IS NOT NULL AND response_version > 0)
    )
) PARTITION BY RANGE (created_utc);

CREATE TABLE IF NOT EXISTS garagemotor.operation_receipts_default
    PARTITION OF garagemotor.operation_receipts DEFAULT;

CREATE INDEX IF NOT EXISTS operation_receipts_lookup_idx
    ON garagemotor.operation_receipts (actor_id, scope_kind, scope_id, operation_key, created_utc DESC);

CREATE INDEX IF NOT EXISTS operation_receipts_garage_time_idx
    ON garagemotor.operation_receipts (garage_id, created_utc DESC)
    WHERE garage_id IS NOT NULL;

ALTER TABLE garagemotor.operation_receipts ENABLE ROW LEVEL SECURITY;
ALTER TABLE garagemotor.operation_receipts FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS operation_receipts_actor_scope ON garagemotor.operation_receipts;
CREATE POLICY operation_receipts_actor_scope ON garagemotor.operation_receipts
    USING (
        actor_id = NULLIF(current_setting('app.actor_id', true), '')::uuid
        AND (
            scope_kind = 'actor'
            OR garage_id = NULLIF(current_setting('app.garage_id', true), '')::uuid
        )
    )
    WITH CHECK (
        actor_id = NULLIF(current_setting('app.actor_id', true), '')::uuid
        AND (
            scope_kind = 'actor'
            OR garage_id = NULLIF(current_setting('app.garage_id', true), '')::uuid
        )
    );

CREATE TABLE IF NOT EXISTS garagemotor.audit_events (
    event_id uuid NOT NULL DEFAULT gen_random_uuid(),
    garage_id uuid,
    occurred_utc timestamptz NOT NULL DEFAULT clock_timestamp(),
    actor_id varchar(100) NOT NULL,
    action varchar(100) NOT NULL,
    target_id varchar(100),
    changed_fields varchar(100)[] NOT NULL DEFAULT ARRAY[]::varchar(100)[],
    trace_id varchar(100),
    PRIMARY KEY (event_id, occurred_utc),
    CONSTRAINT audit_events_changed_fields_valid CHECK (
        cardinality(changed_fields) <= 50 AND array_position(changed_fields, NULL) IS NULL
    )
) PARTITION BY RANGE (occurred_utc);

CREATE TABLE IF NOT EXISTS garagemotor.audit_events_default
    PARTITION OF garagemotor.audit_events DEFAULT;

CREATE INDEX IF NOT EXISTS audit_events_garage_time_idx
    ON garagemotor.audit_events (garage_id, occurred_utc DESC)
    WHERE garage_id IS NOT NULL;

ALTER TABLE garagemotor.audit_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE garagemotor.audit_events FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS audit_events_garage_scope ON garagemotor.audit_events;
CREATE POLICY audit_events_garage_scope ON garagemotor.audit_events
    USING (garage_id = NULLIF(current_setting('app.garage_id', true), '')::uuid)
    WITH CHECK (garage_id = NULLIF(current_setting('app.garage_id', true), '')::uuid);
DROP POLICY IF EXISTS audit_events_migration_retention ON garagemotor.audit_events;
CREATE POLICY audit_events_migration_retention ON garagemotor.audit_events
    TO garagemotor_migrations
    USING (true)
    WITH CHECK (true);

GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA garagemotor TO garagemotor_runtime;
ALTER DEFAULT PRIVILEGES FOR ROLE garagemotor_migrations IN SCHEMA garagemotor
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO garagemotor_runtime;

COMMIT;
