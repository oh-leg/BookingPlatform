using BookingPlatform.BookingService.Domain.Entities;
using BookingPlatform.BookingService.Domain.Enums;

namespace BookingPlatform.BookingService.Application.Interfaces;

public sealed record BookingFilter(
    Guid? UserId,
    Guid? ResourceId,
    BookingStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize);

public interface IBookingRepository
{
    /*Сохраняет бронь, строки, журнал и событие outbox в одной транзакции.
      Бросает SlotUnavailableException при пересечении с активной бронью*/
    Task AddAsync(Booking booking, CancellationToken cancellationToken);

    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /*Сохраняет новый статус и PendingHistory, пишет событие outbox*/
    Task UpdateStatusAsync(Booking booking, CancellationToken cancellationToken);

    Task<IReadOnlyList<Booking>> GetListAsync(BookingFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<BookingHistoryEntry>> GetHistoryAsync(Guid bookingId, CancellationToken cancellationToken);
}
