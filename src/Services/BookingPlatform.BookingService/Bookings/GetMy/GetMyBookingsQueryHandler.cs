using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Application.Users;
using MediatR;

namespace BookingPlatform.BookingService.Bookings.GetMy;

public sealed class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, IReadOnlyList<BookingResponse>>
{
    private readonly IBookingRepository _bookings;
    private readonly CurrentUserResolver _userResolver;

    public GetMyBookingsQueryHandler(IBookingRepository bookings, CurrentUserResolver userResolver)
    {
        _bookings = bookings;
        _userResolver = userResolver;
    }

    public async Task<IReadOnlyList<BookingResponse>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userResolver.ResolveAsync(cancellationToken);

        var bookings = await _bookings.GetListAsync(
            new BookingFilter(user.Id, null, request.Status, request.From, request.To, request.Page, request.PageSize),
            cancellationToken);

        return bookings.Select(BookingResponse.From).ToList();
    }
}
