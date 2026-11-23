using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.Reject;

public sealed record RejectBookingCommand(Guid BookingId, string? Reason) : IRequest<BookingResponse>;
