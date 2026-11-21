using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.GetByResource;

public sealed record GetResourceBookingsQuery(Guid ResourceId, DateTimeOffset From, DateTimeOffset To)
    : IRequest<IReadOnlyList<BookingResponse>>;
