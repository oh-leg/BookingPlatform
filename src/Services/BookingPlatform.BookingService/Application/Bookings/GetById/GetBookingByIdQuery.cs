using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.GetById;

public sealed record GetBookingByIdQuery(Guid BookingId) : IRequest<BookingResponse>;
