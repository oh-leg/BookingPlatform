CREATE EXTENSION IF NOT EXISTS btree_gist;

CREATE TABLE users (
    id          uuid PRIMARY KEY,
    keycloak_id uuid         NOT NULL UNIQUE,
    email       varchar(255) NOT NULL,
    full_name   varchar(255) NOT NULL,
    phone       varchar(20),
    created_at  timestamptz  NOT NULL DEFAULT now(),
    updated_at  timestamptz  NOT NULL DEFAULT now()
);

CREATE TABLE booking_statuses (
    id          int PRIMARY KEY,
    name        varchar(50) NOT NULL UNIQUE,
    description varchar(200),
    is_final    boolean     NOT NULL
);

INSERT INTO booking_statuses (id, name, description, is_final) VALUES
    (1, 'Pending',   'Создана, ждёт подтверждения менеджером', false),
    (2, 'Confirmed', 'Подтверждена',                           false),
    (3, 'Rejected',  'Отклонена менеджером',                   true),
    (4, 'Cancelled', 'Отменена клиентом или менеджером',       true),
    (5, 'Completed', 'Услуга оказана',                         true);

CREATE TABLE bookings (
    id                       uuid PRIMARY KEY,
    user_id                  uuid           NOT NULL REFERENCES users (id),
    resource_id              uuid           NOT NULL,
    offer_id                 uuid           NOT NULL,
    slot_id                  uuid           NOT NULL,
    start_time               timestamptz    NOT NULL,
    end_time                 timestamptz    NOT NULL,
    status_id                int            NOT NULL REFERENCES booking_statuses (id),
    total_price              numeric(12, 2) NOT NULL CHECK (total_price >= 0),
    currency                 varchar(3)     NOT NULL,
    client_name              varchar(255)   NOT NULL,
    client_phone             varchar(20),
    client_email             varchar(255)   NOT NULL,
    comment                  varchar(1000),
    created_at               timestamptz    NOT NULL,
    updated_at               timestamptz    NOT NULL,
    created_by               uuid           NOT NULL REFERENCES users (id),
    updated_by               uuid           NOT NULL REFERENCES users (id),
    blocked_from             timestamptz    NOT NULL,
    blocked_until            timestamptz    NOT NULL,
    cancellation_deadline_at timestamptz    NOT NULL,
    CONSTRAINT bookings_time_check CHECK (end_time > start_time),
    -- главный инвариант: активные брони одного ресурса не пересекаются
    CONSTRAINT bookings_no_overlap EXCLUDE USING gist (
        resource_id WITH =,
        tstzrange(blocked_from, blocked_until) WITH &&
    ) WHERE (status_id IN (1, 2))
);

CREATE INDEX ix_bookings_user_start     ON bookings (user_id, start_time DESC);
CREATE INDEX ix_bookings_resource_start ON bookings (resource_id, start_time);
CREATE INDEX ix_bookings_status         ON bookings (status_id);

CREATE TABLE booking_items (
    id          uuid PRIMARY KEY,
    booking_id  uuid           NOT NULL REFERENCES bookings (id) ON DELETE CASCADE,
    name        varchar(255)   NOT NULL,
    description varchar(500),
    price       numeric(12, 2) NOT NULL CHECK (price >= 0),
    quantity    int            NOT NULL CHECK (quantity > 0),
    total       numeric(12, 2) NOT NULL
);

CREATE INDEX ix_booking_items_booking ON booking_items (booking_id);

CREATE TABLE booking_history (
    id            uuid PRIMARY KEY,
    booking_id    uuid         NOT NULL REFERENCES bookings (id),
    old_status_id int REFERENCES booking_statuses (id),
    new_status_id int          NOT NULL REFERENCES booking_statuses (id),
    reason        varchar(500),
    changed_at    timestamptz  NOT NULL,
    changed_by    uuid REFERENCES users (id)
);

CREATE INDEX ix_booking_history_booking ON booking_history (booking_id, changed_at);

CREATE TABLE outbox_messages (
    id          uuid PRIMARY KEY,
    routing_key varchar(100) NOT NULL,
    payload     jsonb        NOT NULL,
    created_at  timestamptz  NOT NULL,
    published_at timestamptz
);

CREATE INDEX ix_outbox_unpublished ON outbox_messages (created_at) WHERE published_at IS NULL;
