using BookingPlatform.BookingService.Domain.Enums;

namespace BookingPlatform.BookingService.Domain.Entities;

/*Запись журнала смены статуса. Только добавляется, не меняется*/
public sealed class BookingHistoryEntry
{
    private BookingHistoryEntry()
    {
        // Для Dapper
    }

    public Guid Id { get; private set; }

    public Guid BookingId { get; private set; }

    public BookingStatus? OldStatus { get; private set; }

    public BookingStatus NewStatus { get; private set; }

    public string? Reason { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    /*null для действий системы*/
    public Guid? ChangedBy { get; private set; }

    public BookingHistoryEntry(
        Guid bookingId,
        BookingStatus? oldStatus,
        BookingStatus newStatus,
        string? reason,
        DateTimeOffset changedAt,
        Guid? changedBy)
    {
        Id = Guid.NewGuid();
        BookingId = bookingId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Reason = reason;
        ChangedAt = changedAt;
        ChangedBy = changedBy;
    }
}
