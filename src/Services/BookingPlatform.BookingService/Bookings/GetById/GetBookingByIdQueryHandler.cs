using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Application.Users;
using MediatR;

namespace BookingPlatform.BookingService.Bookings.GetById;

public sealed class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingResponse?>
{
    private readonly IBookingRepository _bookings;
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserResolver _userResolver;

    public GetBookingByIdQueryHandler(IBookingRepository bookings, ICurrentUser currentUser, CurrentUserResolver userResolver)
    {
        _bookings = bookings;
        _currentUser = currentUser;
        _userResolver = userResolver;
    }

    public async Task<BookingResponse?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookings.GetByIdAsync(request.BookingId, cancellationToken);

        if (booking is null)
            return null;

        if (!_currentUser.IsManager)
        {
            var user = await _userResolver.ResolveAsync(cancellationToken);

            if (booking.UserId != user.Id)
                throw new UnauthorizedAccessException("Бронь принадлежит другому пользователю.");
        }

        return BookingResponse.From(booking);
    }
}
