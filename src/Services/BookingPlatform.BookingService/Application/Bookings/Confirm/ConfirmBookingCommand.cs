using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.Confirm;

public sealed record ConfirmBookingCommand(Guid BookingId) : IRequest<BookingResponse>;
