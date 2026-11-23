using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Exceptions;
using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.Cancel;

public sealed class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, BookingResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public CancelBookingCommandHandler(IBookingRepository bookingRepository, ICurrentUser currentUser, TimeProvider timeProvider)
    {
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<BookingResponse> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new BookingNotFoundException("Бронь не найдена.");

        booking.Cancel(_currentUser.UserId, _currentUser.IsAdmin, _timeProvider.GetUtcNow(), request.Reason);

        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        return booking.ToResponse();
    }
}
