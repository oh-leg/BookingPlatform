-- Схема BookingService: брони и история изменения их статусов

-- btree_gist нужен, чтобы использовать uuid (оператор =) в GiST exclusion constraint
CREATE EXTENSION IF NOT EXISTS btree_gist;

CREATE TABLE bookings
(
    id                          uuid          NOT NULL PRIMARY KEY,
    user_id                     text          NOT NULL,
    resource_id                 uuid          NOT NULL,
    offer_id                    uuid          NOT NULL,
    start_at                    timestamptz   NOT NULL,
    end_at                      timestamptz   NOT NULL,
    -- Занятый интервал с учетом буферов услуги: [start_at - buffer_before, end_at + buffer_after)
    occupied_range              tstzrange     NOT NULL,
    status                      text          NOT NULL,

    -- Снимок услуги на момент бронирования
    offer_name                  text          NOT NULL,
    duration_minutes            integer       NOT NULL,
    price                       numeric(12,2) NOT NULL,
    currency                    varchar(3)    NOT NULL,
    buffer_before_minutes       integer       NOT NULL,
    buffer_after_minutes        integer       NOT NULL,
    cancellation_deadline_hours integer       NOT NULL,
    requires_confirmation       boolean       NOT NULL,

    comment                     text          NULL,
    created_at                  timestamptz   NOT NULL,
    updated_at                  timestamptz   NOT NULL,
    cancelled_at                timestamptz   NULL,
    cancelled_by                text          NULL,
    cancellation_reason         text          NULL,
    version                     integer       NOT NULL DEFAULT 0,

    CONSTRAINT ck_bookings_status CHECK (status IN ('Pending', 'Confirmed', 'Rejected', 'Cancelled')),
    CONSTRAINT ck_bookings_period CHECK (end_at > start_at),
    CONSTRAINT ck_bookings_occupied_range CHECK (NOT isempty(occupied_range)),
    CONSTRAINT ck_bookings_buffers CHECK (buffer_before_minutes >= 0 AND buffer_after_minutes >= 0),

    -- Защита от двойного бронирования: у одного ресурса не может быть
    -- двух активных броней с пересекающимися занятыми интервалами.
    -- Проверяется атомарно самой БД, в том числе при параллельных вставках.
    CONSTRAINT ex_bookings_no_overlap EXCLUDE USING gist
        (resource_id WITH =, occupied_range WITH &&)
        WHERE (status IN ('Pending', 'Confirmed'))
);

CREATE INDEX ix_bookings_user_start ON bookings (user_id, start_at DESC);
CREATE INDEX ix_bookings_resource_start ON bookings (resource_id, start_at);

-- История изменения статусов: записи только добавляются
CREATE TABLE booking_status_history
(
    id          bigserial   NOT NULL PRIMARY KEY,
    booking_id  uuid        NOT NULL REFERENCES bookings (id),
    from_status text        NULL,
    to_status   text        NOT NULL,
    changed_by  text        NOT NULL,
    changed_at  timestamptz NOT NULL,
    reason      text        NULL
);

CREATE INDEX ix_booking_status_history_booking ON booking_status_history (booking_id, changed_at);
