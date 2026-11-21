using BookingPlatform.BookingService.Domain.Enums;

namespace BookingPlatform.BookingService.Domain.Entities;

/* Запись истории изменения статуса брони (добавляем только) */
public sealed record BookingStatusChange(
    Guid BookingId,
    BookingStatus? FromStatus,
    BookingStatus ToStatus,
    string ChangedBy,
    DateTimeOffset ChangedAt,
    string? Reason);
