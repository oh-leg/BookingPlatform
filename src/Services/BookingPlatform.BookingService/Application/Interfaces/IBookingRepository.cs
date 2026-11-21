using BookingPlatform.BookingService.Domain.Entities;
using BookingPlatform.BookingService.Domain.Enums;
using BookingPlatform.BookingService.Domain.Slots;

namespace BookingPlatform.BookingService.Application.Interfaces;

public interface IBookingRepository
{
    /*Действия выполняем в одной транзакции*/

    /* Сохраняет новую бронь и ее историю. 
       Бросает BookingConflictException, если интервал пересекается с активной бронью ресурса. */
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);

    /* Сохраняет изменения брони и новые записи истории.
       Бросает BookingConflictException, если бронь была изменена параллельно. */
    Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetByUserAsync(
        string userId,
        BookingStatus? status,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetByResourceAsync(
        Guid resourceId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    /* Занятые интервалы активных броней ресурса, пересекающие период */
    Task<IReadOnlyList<TimeInterval>> GetOccupiedIntervalsAsync(
        Guid resourceId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookingStatusChange>> GetHistoryAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
