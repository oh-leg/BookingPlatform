using BookingPlatform.BookingService.Domain.Enums;
using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.GetMy;

public sealed record GetMyBookingsQuery(BookingStatus? Status, DateTimeOffset? From, DateTimeOffset? To) : IRequest<IReadOnlyList<BookingResponse>>;
