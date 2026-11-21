using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Exceptions;
using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.GetHistory;

public sealed class GetBookingHistoryQueryHandler : IRequestHandler<GetBookingHistoryQuery, IReadOnlyList<BookingHistoryItemResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;

    public GetBookingHistoryQueryHandler(IBookingRepository bookingRepository, ICurrentUser currentUser)
    {
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BookingHistoryItemResponse>> Handle(GetBookingHistoryQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new BookingNotFoundException("Бронь не найдена.");

        booking.EnsureCanView(_currentUser.UserId, _currentUser.IsAdmin);

        var history = await _bookingRepository.GetHistoryAsync(booking.Id, cancellationToken);

        return history.Select(x => x.ToResponse()).ToList();
    }
}
