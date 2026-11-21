using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.Cancel;

public sealed record CancelBookingCommand(Guid BookingId, string? Reason) : IRequest<BookingResponse>;
