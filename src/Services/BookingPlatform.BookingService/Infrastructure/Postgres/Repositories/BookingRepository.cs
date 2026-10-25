using System.Data.Common;
using System.Text.Json;
using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Entities;
using BookingPlatform.BookingService.Domain.Enums;
using Dapper;
using Npgsql;

namespace BookingPlatform.BookingService.Infrastructure.Postgres.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private const string ExclusionViolation = "23P01";

    private const string BookingColumns = """
        id, user_id, resource_id, offer_id, slot_id, start_time, end_time, status_id AS status,
        total_price, currency, client_name, client_phone, client_email, comment,
        created_at, updated_at, created_by, updated_by, blocked_from, blocked_until, cancellation_deadline_at
        """;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly DbConnectionFactory _connectionFactory;

    public BookingRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO bookings (
                    id, user_id, resource_id, offer_id, slot_id, start_time, end_time, status_id,
                    total_price, currency, client_name, client_phone, client_email, comment,
                    created_at, updated_at, created_by, updated_by,
                    blocked_from, blocked_until, cancellation_deadline_at)
                VALUES (
                    @Id, @UserId, @ResourceId, @OfferId, @SlotId, @StartTime, @EndTime, @StatusId,
                    @TotalPrice, @Currency, @ClientName, @ClientPhone, @ClientEmail, @Comment,
                    @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy,
                    @BlockedFrom, @BlockedUntil, @CancellationDeadlineAt)
                """,
                new
                {
                    booking.Id, booking.UserId, booking.ResourceId, booking.OfferId, booking.SlotId,
                    booking.StartTime, booking.EndTime, StatusId = (int)booking.Status,
                    booking.TotalPrice, booking.Currency, booking.ClientName, booking.ClientPhone,
                    booking.ClientEmail, booking.Comment, booking.CreatedAt, booking.UpdatedAt,
                    booking.CreatedBy, booking.UpdatedBy, booking.BlockedFrom, booking.BlockedUntil,
                    booking.CancellationDeadlineAt
                },
                transaction, cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition("""
                INSERT INTO booking_items (id, booking_id, name, description, price, quantity, total)
                VALUES (@Id, @BookingId, @Name, @Description, @Price, @Quantity, @Total)
                """, booking.Items, transaction, cancellationToken: cancellationToken));

            await InsertHistoryAsync(connection, transaction, booking, cancellationToken);
            await EnqueueEventAsync(connection, transaction, booking, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == ExclusionViolation)
        {
            throw new SlotUnavailableException();
        }
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken);

        var booking = await connection.QuerySingleOrDefaultAsync<Booking>(new CommandDefinition(
            $"SELECT {BookingColumns} FROM bookings WHERE id = @id",
            new { id }, cancellationToken: cancellationToken));

        if (booking is not null)
            await LoadItemsAsync(connection, [booking], cancellationToken);

        return booking;
    }

    public async Task UpdateStatusAsync(Booking booking, CancellationToken cancellationToken)
    {
        var first = booking.PendingHistory.FirstOrDefault();

        if (first?.OldStatus is null)
            throw new InvalidOperationException("Нет изменения статуса для сохранения.");

        await using var connection = await _connectionFactory.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        // условие по прежнему статусу защищает от параллельной смены статуса
        var updated = await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE bookings
            SET status_id = @StatusId, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND status_id = @OldStatusId
            """,
            new
            {
                booking.Id, StatusId = (int)booking.Status, booking.UpdatedAt, booking.UpdatedBy,
                OldStatusId = (int)first.OldStatus.Value
            },
            transaction, cancellationToken: cancellationToken));

        if (updated == 0)
            throw new InvalidOperationException("Статус брони уже был изменён, обновите данные и повторите.");

        await InsertHistoryAsync(connection, transaction, booking, cancellationToken);
        await EnqueueEventAsync(connection, transaction, booking, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetListAsync(BookingFilter filter, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken);

        var bookings = (await connection.QueryAsync<Booking>(new CommandDefinition($"""
            SELECT {BookingColumns} FROM bookings
            WHERE (@UserId::uuid IS NULL OR user_id = @UserId::uuid)
              AND (@ResourceId::uuid IS NULL OR resource_id = @ResourceId::uuid)
              AND (@StatusId::int IS NULL OR status_id = @StatusId::int)
              AND (@From::timestamptz IS NULL OR start_time >= @From::timestamptz)
              AND (@To::timestamptz IS NULL OR start_time < @To::timestamptz)
            ORDER BY start_time DESC
            LIMIT @PageSize OFFSET @Offset
            """,
            new
            {
                filter.UserId,
                filter.ResourceId,
                StatusId = (int?)filter.Status,
                From = filter.From?.UtcDateTime,
                To = filter.To?.UtcDateTime,
                filter.PageSize,
                Offset = (filter.Page - 1) * filter.PageSize
            },
            cancellationToken: cancellationToken))).ToList();

        await LoadItemsAsync(connection, bookings, cancellationToken);

        return bookings;
    }

    public async Task<IReadOnlyList<BookingHistoryEntry>> GetHistoryAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken);

        var entries = await connection.QueryAsync<BookingHistoryEntry>(new CommandDefinition("""
            SELECT id, booking_id, old_status_id AS old_status, new_status_id AS new_status,
                   reason, changed_at, changed_by
            FROM booking_history
            WHERE booking_id = @bookingId
            ORDER BY changed_at
            """, new { bookingId }, cancellationToken: cancellationToken));

        return entries.ToList();
    }

    private static async Task LoadItemsAsync(DbConnection connection, IReadOnlyList<Booking> bookings, CancellationToken cancellationToken)
    {
        if (bookings.Count == 0)
            return;

        var items = await connection.QueryAsync<BookingItem>(new CommandDefinition("""
            SELECT id, booking_id, name, description, price, quantity, total
            FROM booking_items
            WHERE booking_id = ANY(@ids)
            """, new { ids = bookings.Select(b => b.Id).ToArray() }, cancellationToken: cancellationToken));

        var byBooking = items.ToLookup(i => i.BookingId);

        foreach (var booking in bookings)
            booking.LoadItems(byBooking[booking.Id]);
    }

    private static Task InsertHistoryAsync(DbConnection connection, DbTransaction transaction, Booking booking, CancellationToken cancellationToken)
    {
        var rows = booking.PendingHistory.Select(h => new
        {
            h.Id,
            h.BookingId,
            OldStatusId = (int?)h.OldStatus,
            NewStatusId = (int)h.NewStatus,
            h.Reason,
            h.ChangedAt,
            h.ChangedBy
        });

        return connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO booking_history (id, booking_id, old_status_id, new_status_id, reason, changed_at, changed_by)
            VALUES (@Id, @BookingId, @OldStatusId, @NewStatusId, @Reason, @ChangedAt, @ChangedBy)
            """, rows, transaction, cancellationToken: cancellationToken));
    }

    /*Событие пишется в той же транзакции, что и бронь. Отправка в RabbitMQ будет отдельной задачей*/
    private static async Task EnqueueEventAsync(DbConnection connection, DbTransaction transaction, Booking booking, CancellationToken cancellationToken)
    {
        var owner = await connection.QuerySingleAsync<Guid>(new CommandDefinition(
            "SELECT keycloak_id FROM users WHERE id = @UserId",
            new { booking.UserId }, transaction, cancellationToken: cancellationToken));

        var last = booking.PendingHistory.Last();
        var eventId = Guid.NewGuid();

        var payload = JsonSerializer.Serialize(new
        {
            eventId,
            occurredAt = last.ChangedAt.UtcDateTime,
            bookingId = booking.Id,
            status = booking.Status.ToString(),
            userId = owner,
            resourceId = booking.ResourceId,
            offerName = booking.Items.FirstOrDefault()?.Name,
            startTime = booking.StartTime.UtcDateTime,
            endTime = booking.EndTime.UtcDateTime,
            totalPrice = booking.TotalPrice,
            currency = booking.Currency,
            clientName = booking.ClientName,
            clientEmail = booking.ClientEmail,
            clientPhone = booking.ClientPhone,
            reason = last.Reason
        }, JsonOptions);

        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO outbox_messages (id, routing_key, payload, created_at)
            VALUES (@eventId, @routingKey, CAST(@payload AS jsonb), @createdAt)
            """,
            new { eventId, routingKey = RoutingKey(booking.Status), payload, createdAt = last.ChangedAt },
            transaction, cancellationToken: cancellationToken));
    }

    private static string RoutingKey(BookingStatus status) => status switch
    {
        BookingStatus.Pending => "booking.created",
        BookingStatus.Confirmed => "booking.confirmed",
        BookingStatus.Rejected => "booking.rejected",
        BookingStatus.Cancelled => "booking.cancelled",
        BookingStatus.Completed => "booking.completed",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };
}
