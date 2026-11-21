using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Exceptions;
using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.Reject;

public sealed class RejectBookingCommandHandler : IRequestHandler<RejectBookingCommand, BookingResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public RejectBookingCommandHandler(IBookingRepository bookingRepository, ICurrentUser currentUser, TimeProvider timeProvider)
    {
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<BookingResponse> Handle(RejectBookingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
            throw new ForbiddenBookingActionException("Отклонять брони может только администратор.");

        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new BookingNotFoundException("Бронь не найдена.");

        booking.Reject(_currentUser.UserId, _timeProvider.GetUtcNow(), request.Reason);

        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        return booking.ToResponse();
    }
}
