using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.Create;

public sealed record CreateBookingCommand(Guid ResourceId, Guid OfferId, DateTimeOffset StartAt, string? Comment) : IRequest<BookingResponse>;
