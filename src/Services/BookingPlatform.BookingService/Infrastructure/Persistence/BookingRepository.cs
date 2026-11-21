using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Entities;
using BookingPlatform.BookingService.Domain.Enums;
using BookingPlatform.BookingService.Domain.Exceptions;
using BookingPlatform.BookingService.Domain.Slots;
using Dapper;
using Npgsql;

namespace BookingPlatform.BookingService.Infrastructure.Persistence;

public sealed class BookingRepository : IBookingRepository
{
    private const string SelectBookings =
        """
        SELECT id                          AS Id,
               user_id                     AS UserId,
               resource_id                 AS ResourceId,
               offer_id                    AS OfferId,
               start_at                    AS StartAt,
               end_at                      AS EndAt,
               status                      AS Status,
               offer_name                  AS OfferName,
               duration_minutes            AS DurationMinutes,
               price                       AS Price,
               currency                    AS Currency,
               buffer_before_minutes       AS BufferBeforeMinutes,
               buffer_after_minutes        AS BufferAfterMinutes,
               cancellation_deadline_hours AS CancellationDeadlineHours,
               requires_confirmation       AS RequiresConfirmation,
               comment                     AS Comment,
               created_at                  AS CreatedAt,
               updated_at                  AS UpdatedAt,
               cancelled_at                AS CancelledAt,
               cancelled_by                AS CancelledBy,
               cancellation_reason         AS CancellationReason,
               version                     AS Version
        FROM bookings
        """;

    private const string ActiveStatusesSql = "('Pending', 'Confirmed')";

    private readonly NpgsqlDataSource _dataSource;

    public BookingRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(booking);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO bookings (
                    id, user_id, resource_id, offer_id, start_at, end_at, occupied_range, status,
                    offer_name, duration_minutes, price, currency, buffer_before_minutes, buffer_after_minutes,
                    cancellation_deadline_hours, requires_confirmation, comment, created_at, updated_at, version)
                VALUES (
                    @Id, @UserId, @ResourceId, @OfferId, @StartAt, @EndAt, tstzrange(@OccupiedFrom, @OccupiedTo, '[)'), @Status,
                    @OfferName, @DurationMinutes, @Price, @Currency, @BufferBeforeMinutes, @BufferAfterMinutes,
                    @CancellationDeadlineHours, @RequiresConfirmation, @Comment, @CreatedAt, @UpdatedAt, 0)
                """,
                new
                {
                    booking.Id,
                    booking.UserId,
                    booking.ResourceId,
                    booking.Offer.OfferId,
                    StartAt = ToUtc(booking.StartAt),
                    EndAt = ToUtc(booking.EndAt),
                    OccupiedFrom = ToUtc(booking.OccupiedFrom),
                    OccupiedTo = ToUtc(booking.OccupiedTo),
                    Status = booking.Status.ToString(),
                    booking.Offer.OfferName,
                    booking.Offer.DurationMinutes,
                    booking.Offer.Price,
                    booking.Offer.Currency,
                    booking.Offer.BufferBeforeMinutes,
                    booking.Offer.BufferAfterMinutes,
                    booking.Offer.CancellationDeadlineHours,
                    booking.Offer.RequiresConfirmation,
                    booking.Comment,
                    CreatedAt = ToUtc(booking.CreatedAt),
                    UpdatedAt = ToUtc(booking.UpdatedAt)
                },
                transaction,
                cancellationToken: cancellationToken));

            await InsertHistoryAsync(connection, transaction, booking.PendingHistory, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ExclusionViolation)
        {
            throw new BookingConflictException("Выбранное время уже забронировано.", ex);
        }

        booking.MarkPersisted(0);
    }

    public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(booking);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        int affected;

        try
        {
            affected = await connection.ExecuteAsync(new CommandDefinition(
                """
                UPDATE bookings
                SET status              = @Status,
                    updated_at          = @UpdatedAt,
                    cancelled_at        = @CancelledAt,
                    cancelled_by        = @CancelledBy,
                    cancellation_reason = @CancellationReason,
                    version             = version + 1
                WHERE id = @Id AND version = @Version
                """,
                new
                {
                    booking.Id,
                    booking.Version,
                    Status = booking.Status.ToString(),
                    UpdatedAt = ToUtc(booking.UpdatedAt),
                    CancelledAt = booking.CancelledAt.HasValue ? ToUtc(booking.CancelledAt.Value) : (DateTime?)null,
                    booking.CancelledBy,
                    booking.CancellationReason
                },
                transaction,
                cancellationToken: cancellationToken));

            if (affected == 0)
                throw new BookingConflictException("Бронь была изменена другим запросом. Повторите операцию.");

            await InsertHistoryAsync(connection, transaction, booking.PendingHistory, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ExclusionViolation)
        {
            throw new BookingConflictException("Выбранное время уже забронировано.", ex);
        }

        booking.MarkPersisted(booking.Version + 1);
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<BookingRow>(new CommandDefinition(
            SelectBookings + " WHERE id = @id",
            new { id },
            cancellationToken: cancellationToken));

        return row?.ToDomain();
    }

    public async Task<IReadOnlyList<Booking>> GetByUserAsync(
        string userId,
        BookingStatus? status,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<BookingRow>(new CommandDefinition(
            SelectBookings +
            """
             WHERE user_id = @userId
               AND (@status::text IS NULL OR status = @status::text)
               AND (@from::timestamptz IS NULL OR end_at > @from::timestamptz)
               AND (@to::timestamptz IS NULL OR start_at < @to::timestamptz)
             ORDER BY start_at DESC
            """,
            new
            {
                userId,
                status = status?.ToString(),
                from = from.HasValue ? ToUtc(from.Value) : (DateTime?)null,
                to = to.HasValue ? ToUtc(to.Value) : (DateTime?)null
            },
            cancellationToken: cancellationToken));

        return rows.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Booking>> GetByResourceAsync(
        Guid resourceId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<BookingRow>(new CommandDefinition(
            SelectBookings +
            """
             WHERE resource_id = @resourceId
               AND end_at > @from
               AND start_at < @to
             ORDER BY start_at
            """,
            new { resourceId, from = ToUtc(from), to = ToUtc(to) },
            cancellationToken: cancellationToken));

        return rows.Select(x => x.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<TimeInterval>> GetOccupiedIntervalsAsync(
        Guid resourceId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<(DateTime From, DateTime To)>(new CommandDefinition(
            $"""
             SELECT lower(occupied_range) AS range_from, upper(occupied_range) AS range_to
             FROM bookings
             WHERE resource_id = @resourceId
               AND status IN {ActiveStatusesSql}
               AND occupied_range && tstzrange(@from, @to, '[)')
             """,
            new { resourceId, from = ToUtc(from), to = ToUtc(to) },
            cancellationToken: cancellationToken));

        return rows.Select(x => new TimeInterval(FromUtc(x.From), FromUtc(x.To))).ToList();
    }

    public async Task<IReadOnlyList<BookingStatusChange>> GetHistoryAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<HistoryRow>(new CommandDefinition(
            """
            SELECT booking_id  AS BookingId,
                   from_status AS FromStatus,
                   to_status   AS ToStatus,
                   changed_by  AS ChangedBy,
                   changed_at  AS ChangedAt,
                   reason      AS Reason
            FROM booking_status_history
            WHERE booking_id = @bookingId
            ORDER BY changed_at, id
            """,
            new { bookingId },
            cancellationToken: cancellationToken));

        return rows.Select(x => new BookingStatusChange(
                x.BookingId,
                x.FromStatus is null ? null : Enum.Parse<BookingStatus>(x.FromStatus),
                Enum.Parse<BookingStatus>(x.ToStatus),
                x.ChangedBy,
                FromUtc(x.ChangedAt),
                x.Reason))
            .ToList();
    }

    private static Task InsertHistoryAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        IReadOnlyCollection<BookingStatusChange> changes,
        CancellationToken cancellationToken)
    {
        if (changes.Count == 0)
            return Task.CompletedTask;

        return connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO booking_status_history (booking_id, from_status, to_status, changed_by, changed_at, reason)
            VALUES (@BookingId, @FromStatus, @ToStatus, @ChangedBy, @ChangedAt, @Reason)
            """,
            changes.Select(x => new
            {
                x.BookingId,
                FromStatus = x.FromStatus?.ToString(),
                ToStatus = x.ToStatus.ToString(),
                x.ChangedBy,
                ChangedAt = ToUtc(x.ChangedAt),
                x.Reason
            }),
            transaction,
            cancellationToken: cancellationToken));
    }

    /* Npgsql принимает для timestamptz только DateTime с Kind=Utc */
    private static DateTime ToUtc(DateTimeOffset value) => value.UtcDateTime;

    private static DateTimeOffset FromUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc), TimeSpan.Zero);

    private sealed class BookingRow
    {
        public Guid Id { get; init; }
        public string UserId { get; init; } = null!;
        public Guid ResourceId { get; init; }
        public Guid OfferId { get; init; }
        public DateTime StartAt { get; init; }
        public DateTime EndAt { get; init; }
        public string Status { get; init; } = null!;
        public string OfferName { get; init; } = null!;
        public int DurationMinutes { get; init; }
        public decimal Price { get; init; }
        public string Currency { get; init; } = null!;
        public int BufferBeforeMinutes { get; init; }
        public int BufferAfterMinutes { get; init; }
        public int CancellationDeadlineHours { get; init; }
        public bool RequiresConfirmation { get; init; }
        public string? Comment { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public DateTime? CancelledAt { get; init; }
        public string? CancelledBy { get; init; }
        public string? CancellationReason { get; init; }
        public int Version { get; init; }

        public Booking ToDomain() => Booking.Restore(
            Id,
            UserId,
            ResourceId,
            new BookedOfferSnapshot(
                OfferId,
                OfferName,
                DurationMinutes,
                Price,
                Currency,
                BufferBeforeMinutes,
                BufferAfterMinutes,
                CancellationDeadlineHours,
                RequiresConfirmation),
            FromUtc(StartAt),
            FromUtc(EndAt),
            Enum.Parse<BookingStatus>(Status),
            Comment,
            FromUtc(CreatedAt),
            FromUtc(UpdatedAt),
            CancelledAt.HasValue ? FromUtc(CancelledAt.Value) : null,
            CancelledBy,
            CancellationReason,
            Version);
    }

    private sealed class HistoryRow
    {
        public Guid BookingId { get; init; }
        public string? FromStatus { get; init; }
        public string ToStatus { get; init; } = null!;
        public string ChangedBy { get; init; } = null!;
        public DateTime ChangedAt { get; init; }
        public string? Reason { get; init; }
    }
}
