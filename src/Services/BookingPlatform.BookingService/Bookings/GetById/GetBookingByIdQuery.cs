using MediatR;

namespace BookingPlatform.BookingService.Bookings.GetById;

public sealed record GetBookingByIdQuery(Guid BookingId) : IRequest<BookingResponse?>;
