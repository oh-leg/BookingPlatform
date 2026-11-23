using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Exceptions;
using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.Confirm;

public sealed class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand, BookingResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public ConfirmBookingCommandHandler(IBookingRepository bookingRepository, ICurrentUser currentUser, TimeProvider timeProvider)
    {
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<BookingResponse> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
            throw new ForbiddenBookingActionException("Подтверждать брони может только администратор.");

        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new BookingNotFoundException("Бронь не найдена.");

        booking.Confirm(_currentUser.UserId, _timeProvider.GetUtcNow());

        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        return booking.ToResponse();
    }
}
