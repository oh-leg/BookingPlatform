using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.GetHistory;

public sealed record GetBookingHistoryQuery(Guid BookingId) : IRequest<IReadOnlyList<BookingHistoryItemResponse>>;
