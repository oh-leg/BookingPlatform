using BookingPlatform.BookingService.Domain.Enums;

namespace BookingPlatform.BookingService.Domain.Entities;

public sealed class Booking
{
    private readonly List<BookingItem> _items = [];
    private readonly List<BookingHistoryEntry> _pendingHistory = [];

    public IReadOnlyCollection<BookingItem> Items => _items.AsReadOnly();

    /*записи журнала, добавленные в этой сессии и ещё не сохранённые в БД*/
    public IReadOnlyCollection<BookingHistoryEntry> PendingHistory => _pendingHistory.AsReadOnly();

    private Booking()
    {
        // Для Dapper
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid ResourceId { get; private set; }

    public Guid OfferId { get; private set; }

    public Guid SlotId { get; private set; }

    public DateTimeOffset StartTime { get; private set; }

    public DateTimeOffset EndTime { get; private set; }

    public BookingStatus Status { get; private set; }

    public decimal TotalPrice { get; private set; }

    public string Currency { get; private set; } = null!;

    public string ClientName { get; private set; } = null!;

    public string? ClientPhone { get; private set; }

    public string ClientEmail { get; private set; } = null!;

    public string? Comment { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public Guid UpdatedBy { get; private set; }

    /*занятый интервал с учётом подготовки до и после услуги*/
    public DateTimeOffset BlockedFrom { get; private set; }

    public DateTimeOffset BlockedUntil { get; private set; }

    /*до этого момента клиент может отменить бронь сам*/
    public DateTimeOffset CancellationDeadlineAt { get; private set; }

    public Booking(
        Guid userId,
        Guid resourceId,
        Guid offerId,
        DateTimeOffset startTime,
        OfferSnapshot offer,
        string clientName,
        string clientEmail,
        string? clientPhone,
        string? comment,
        DateTimeOffset now,
        Guid? createdBy = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Идентификатор пользователя не может быть пустым.", nameof(userId));

        if (resourceId == Guid.Empty)
            throw new ArgumentException("Идентификатор ресурса не может быть пустым.", nameof(resourceId));

        if (offerId == Guid.Empty)
            throw new ArgumentException("Идентификатор услуги не может быть пустым.", nameof(offerId));

        ArgumentNullException.ThrowIfNull(offer);

        if (string.IsNullOrWhiteSpace(clientName))
            throw new ArgumentException("Имя клиента не может быть пустым.", nameof(clientName));

        if (string.IsNullOrWhiteSpace(clientEmail))
            throw new ArgumentException("Email клиента не может быть пустым.", nameof(clientEmail));

        if (offer.DurationMinutes <= 0)
            throw new ArgumentException("Время выполнения должно быть больше нуля.", nameof(offer));

        if (offer.Price < 0)
            throw new ArgumentException("Цена не может быть отрицательной.", nameof(offer));

        if (offer.BufferBeforeMinutes < 0 || offer.BufferAfterMinutes < 0)
            throw new ArgumentException("Время подготовки не может быть отрицательным.", nameof(offer));

        if (offer.CancellationDeadlineHours < 0)
            throw new ArgumentException("Срок отказа от услуги не может быть отрицательным.", nameof(offer));

        if (string.IsNullOrWhiteSpace(offer.Currency) || offer.Currency.Trim().Length != 3)
            throw new ArgumentException("Валюта должна быть кодом ISO 4217.", nameof(offer));

        if (comment is { Length: > 1000 })
            throw new ArgumentException("Комментарий не может быть длиннее 1000 символов.", nameof(comment));

        var start = startTime.ToUniversalTime();

        if (start <= now)
            throw new ArgumentException("Нельзя забронировать время в прошлом.", nameof(startTime));

        Id = Guid.NewGuid();
        UserId = userId;
        ResourceId = resourceId;
        OfferId = offerId;
        StartTime = start;
        EndTime = start.AddMinutes(offer.DurationMinutes);
        SlotId = Services.SlotId.Create(resourceId, offerId, start);
        BlockedFrom = start.AddMinutes(-offer.BufferBeforeMinutes);
        BlockedUntil = EndTime.AddMinutes(offer.BufferAfterMinutes);
        CancellationDeadlineAt = start.AddHours(-offer.CancellationDeadlineHours);
        Currency = offer.Currency.Trim().ToUpperInvariant();
        ClientName = clientName.Trim();
        ClientEmail = clientEmail.Trim();
        ClientPhone = string.IsNullOrWhiteSpace(clientPhone) ? null : clientPhone.Trim();
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        CreatedAt = UpdatedAt = now;
        CreatedBy = UpdatedBy = createdBy ?? userId;

        var item = new BookingItem(Id, offer.Name, offer.Description, offer.Price);
        _items.Add(item);
        TotalPrice = item.Total;

        Status = offer.RequiresConfirmation ? BookingStatus.Pending : BookingStatus.Confirmed;
        _pendingHistory.Add(new BookingHistoryEntry(Id, null, Status, null, now, UpdatedBy));
    }

    /*заполнение строк брони при чтении из БД*/
    public void LoadItems(IEnumerable<BookingItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }

    public void Confirm(Guid changedBy, DateTimeOffset now)
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Подтвердить можно только бронь в статусе Pending, текущий: {Status}.");

        ChangeStatus(BookingStatus.Confirmed, null, changedBy, now);
    }

    public void Reject(Guid changedBy, string reason, DateTimeOffset now)
    {
        RequireReason(reason);

        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Отклонить можно только бронь в статусе Pending, текущий: {Status}.");

        ChangeStatus(BookingStatus.Rejected, reason, changedBy, now);
    }

    /*Менеджер отменяет всегда, клиент только свою бронь и до срока отмены*/
    public void Cancel(Guid changedBy, bool isManager, string reason, DateTimeOffset now)
    {
        RequireReason(reason);

        if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            throw new InvalidOperationException($"Отменить можно бронь в статусе Pending или Confirmed, текущий: {Status}.");

        if (!isManager)
        {
            if (changedBy != UserId)
                throw new UnauthorizedAccessException("Отменить чужую бронь может только менеджер.");

            if (now > CancellationDeadlineAt)
                throw new InvalidOperationException("Срок самостоятельной отмены брони истёк.");
        }

        ChangeStatus(BookingStatus.Cancelled, reason, changedBy, now);
    }

    /*changedBy = null для автозавершения системой*/
    public void Complete(Guid? changedBy, DateTimeOffset now)
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException($"Завершить можно только бронь в статусе Confirmed, текущий: {Status}.");

        ChangeStatus(BookingStatus.Completed, null, changedBy, now);
    }

    private void ChangeStatus(BookingStatus newStatus, string? reason, Guid? changedBy, DateTimeOffset now)
    {
        _pendingHistory.Add(new BookingHistoryEntry(Id, Status, newStatus, reason?.Trim(), now, changedBy));

        Status = newStatus;
        UpdatedAt = now;

        if (changedBy is { } id)
            UpdatedBy = id;
    }

    private static void RequireReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Нужно указать причину.", nameof(reason));

        if (reason.Length > 500)
            throw new ArgumentException("Причина не может быть длиннее 500 символов.", nameof(reason));
    }
}
