using BookingPlatform.BookingService.Domain.Enums;
using BookingPlatform.BookingService.Domain.Exceptions;

namespace BookingPlatform.BookingService.Domain.Entities;

/*
 * Агрегат бронирования, который управляет состоянием, проверяет бизнес-правила и собирает историю изменений.
 */
public sealed class Booking
{
    private readonly List<BookingStatusChange> _pendingHistory = [];

    /* Конструктор для восстановления из БД */
    private Booking()
    {
    }

    public Guid Id { get; private set; }

    public string UserId { get; private set; } = null!;

    public Guid ResourceId { get; private set; }

    public BookedOfferSnapshot Offer { get; private set; } = null!;

    public DateTimeOffset StartAt { get; private set; }

    public DateTimeOffset EndAt { get; private set; }

    /* Начало занятого интервала с учетом буфера перед услугой */
    public DateTimeOffset OccupiedFrom => StartAt.AddMinutes(-Offer.BufferBeforeMinutes);

    /* Конец занятого интервала с учетом буфера после услуги */
    public DateTimeOffset OccupiedTo => EndAt.AddMinutes(Offer.BufferAfterMinutes);

    public BookingStatus Status { get; private set; }

    public string? Comment { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public string? CancelledBy { get; private set; }

    public string? CancellationReason { get; private set; }

    /* Версия для блокировки(оптимистической. подстраховка от одновременного редактирования одной записи несколькми пользователями) */
    public int Version { get; private set; }

    public bool IsActive => Status is BookingStatus.Pending or BookingStatus.Confirmed;

    /* Крайний срок отмены брони владельцем */
    public DateTimeOffset CancellationDeadline => StartAt.AddHours(-Offer.CancellationDeadlineHours);

    /* Изменения статуса, которые еще не сохранены в историю */
    public IReadOnlyCollection<BookingStatusChange> PendingHistory => _pendingHistory.AsReadOnly();

    public static Booking Create(
        Guid id,
        string userId,
        Guid resourceId,
        BookedOfferSnapshot offer,
        DateTimeOffset startAt,
        DateTimeOffset now,
        string? comment = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Идентификатор брони не может быть пустым.", nameof(id));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("Идентификатор пользователя не может быть пустым.", nameof(userId));

        if (resourceId == Guid.Empty)
            throw new ArgumentException("Идентификатор ресурса не может быть пустым.", nameof(resourceId));

        if (offer is null)
            throw new ArgumentException("Необходимо указать услугу.", nameof(offer));

        if (offer.OfferId == Guid.Empty)
            throw new ArgumentException("Идентификатор услуги не может быть пустым.", nameof(offer));

        if (offer.DurationMinutes <= 0)
            throw new ArgumentException("Длительность услуги должна быть больше нуля.", nameof(offer));

        if (offer.BufferBeforeMinutes < 0 || offer.BufferAfterMinutes < 0)
            throw new ArgumentException("Буферы услуги не могут быть отрицательными.", nameof(offer));

        if (offer.CancellationDeadlineHours < 0)
            throw new ArgumentException("Срок отмены не может быть отрицательным.", nameof(offer));

        if (startAt <= now)
            throw new ArgumentException("Нельзя забронировать время в прошлом.", nameof(startAt));

        var status = offer.RequiresConfirmation ? BookingStatus.Pending : BookingStatus.Confirmed;
        var start = startAt.ToUniversalTime();

        var booking = new Booking
        {
            Id = id,
            UserId = userId,
            ResourceId = resourceId,
            Offer = offer,
            StartAt = start,
            EndAt = start.AddMinutes(offer.DurationMinutes),
            Status = status,
            Comment = Normalize(comment),
            CreatedAt = now,
            UpdatedAt = now,
            Version = 0
        };

        booking._pendingHistory.Add(new BookingStatusChange(id, null, status, userId, now, null));

        return booking;
    }

    /* Восстановление брони из хранилища*/
    public static Booking Restore(
        Guid id,
        string userId,
        Guid resourceId,
        BookedOfferSnapshot offer,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        BookingStatus status,
        string? comment,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? cancelledAt,
        string? cancelledBy,
        string? cancellationReason,
        int version)
    {
        return new Booking
        {
            Id = id,
            UserId = userId,
            ResourceId = resourceId,
            Offer = offer,
            StartAt = startAt,
            EndAt = endAt,
            Status = status,
            Comment = comment,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CancelledAt = cancelledAt,
            CancelledBy = cancelledBy,
            CancellationReason = cancellationReason,
            Version = version
        };
    }

    /* Проверка, что пользователь может видеть бронь: владелец или администратор */
    public void EnsureCanView(string userId, bool isAdmin)
    {
        if (!isAdmin && !string.Equals(UserId, userId, StringComparison.Ordinal))
            throw new ForbiddenBookingActionException("Нет доступа к брони.");
    }

    public void Confirm(string changedBy, DateTimeOffset now)
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Подтвердить можно только бронь в статусе ожидания.");

        ChangeStatus(BookingStatus.Confirmed, changedBy, now, null);
    }

    public void Reject(string changedBy, DateTimeOffset now, string? reason)
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Отклонить можно только бронь в статусе ожидания.");

        ChangeStatus(BookingStatus.Rejected, changedBy, now, reason);
    }

    /* Отмена брони: владелец - до дедлайна отмены, администратор - в любое время */
    public void Cancel(string userId, bool isAdmin, DateTimeOffset now, string? reason)
    {
        EnsureCanView(userId, isAdmin);

        if (!IsActive)
            throw new InvalidOperationException("Бронь уже отменена или отклонена.");

        if (!isAdmin && now >= CancellationDeadline)
            throw new InvalidOperationException("Срок отмены брони истек.");

        ChangeStatus(BookingStatus.Cancelled, userId, now, reason);

        CancelledAt = now;
        CancelledBy = userId;
        CancellationReason = Normalize(reason);
    }

    /* Вызывается репозиторием после успешного сохранения */
    public void MarkPersisted(int newVersion)
    {
        Version = newVersion;
        _pendingHistory.Clear();
    }

    private void ChangeStatus(BookingStatus newStatus, string changedBy, DateTimeOffset now, string? reason)
    {
        if (string.IsNullOrWhiteSpace(changedBy))
            throw new ArgumentException("Необходимо указать автора изменения.", nameof(changedBy));

        _pendingHistory.Add(new BookingStatusChange(Id, Status, newStatus, changedBy, now, Normalize(reason)));
        Status = newStatus;
        UpdatedAt = now;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
