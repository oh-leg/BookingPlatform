using MediatR;

namespace BookingPlatform.BookingService.Bookings.Confirm;

public sealed record ConfirmBookingCommand(Guid BookingId) : IRequest<BookingResponse>;
