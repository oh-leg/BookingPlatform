using BookingPlatform.BookingService.Domain.Enums;
using MediatR;

namespace BookingPlatform.BookingService.Bookings.GetAll;

public sealed record GetAllBookingsQuery(
    Guid? ResourceId,
    BookingStatus? Status,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize) : IRequest<IReadOnlyList<BookingResponse>>;
