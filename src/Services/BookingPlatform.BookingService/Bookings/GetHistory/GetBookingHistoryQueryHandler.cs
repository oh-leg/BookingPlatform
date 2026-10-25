using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Application.Users;
using MediatR;

namespace BookingPlatform.BookingService.Bookings.GetHistory;

public sealed class GetBookingHistoryQueryHandler : IRequestHandler<GetBookingHistoryQuery, IReadOnlyList<BookingHistoryResponse>?>
{
    private readonly IBookingRepository _bookings;
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserResolver _userResolver;

    public GetBookingHistoryQueryHandler(IBookingRepository bookings, ICurrentUser currentUser, CurrentUserResolver userResolver)
    {
        _bookings = bookings;
        _currentUser = currentUser;
        _userResolver = userResolver;
    }

    public async Task<IReadOnlyList<BookingHistoryResponse>?> Handle(GetBookingHistoryQuery request, CancellationToken cancellationToken)
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

        var history = await _bookings.GetHistoryAsync(request.BookingId, cancellationToken);

        return history
            .Select(h => new BookingHistoryResponse(h.OldStatus?.ToString(), h.NewStatus.ToString(), h.Reason, h.ChangedAt, h.ChangedBy))
            .ToList();
    }
}
