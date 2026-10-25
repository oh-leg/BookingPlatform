using BookingPlatform.BookingService.Domain.Enums;
using MediatR;

namespace BookingPlatform.BookingService.Bookings.GetMy;

public sealed record GetMyBookingsQuery(
    BookingStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize) : IRequest<IReadOnlyList<BookingResponse>>;
