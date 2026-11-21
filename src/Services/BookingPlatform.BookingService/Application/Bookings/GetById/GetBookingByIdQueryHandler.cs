using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Exceptions;
using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.GetById;

public sealed class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;

    public GetBookingByIdQueryHandler(IBookingRepository bookingRepository, ICurrentUser currentUser)
    {
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
    }

    public async Task<BookingResponse> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken) ?? throw new BookingNotFoundException("Бронь не найдена.");

        booking.EnsureCanView(_currentUser.UserId, _currentUser.IsAdmin);

        return booking.ToResponse();
    }
}
