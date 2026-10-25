using MediatR;

namespace BookingPlatform.BookingService.Bookings.GetHistory;

public sealed record GetBookingHistoryQuery(Guid BookingId) : IRequest<IReadOnlyList<BookingHistoryResponse>?>;
