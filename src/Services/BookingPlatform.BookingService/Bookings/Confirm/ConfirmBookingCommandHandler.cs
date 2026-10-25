using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Application.Users;
using MediatR;

namespace BookingPlatform.BookingService.Bookings.Confirm;

public sealed class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand, BookingResponse>
{
    private readonly IBookingRepository _bookings;
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserResolver _userResolver;

    public ConfirmBookingCommandHandler(IBookingRepository bookings, ICurrentUser currentUser, CurrentUserResolver userResolver)
    {
        _bookings = bookings;
        _currentUser = currentUser;
        _userResolver = userResolver;
    }

    public async Task<BookingResponse> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsManager)
            throw new UnauthorizedAccessException("Действие доступно только менеджеру.");

        var booking = await _bookings.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new KeyNotFoundException("Бронь не найдена.");

        var user = await _userResolver.ResolveAsync(cancellationToken);

        booking.Confirm(user.Id, DateTimeOffset.UtcNow);

        await _bookings.UpdateStatusAsync(booking, cancellationToken);

        return BookingResponse.From(booking);
    }
}
