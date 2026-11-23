using BookingPlatform.BookingService.Application.Interfaces;
using MediatR;

namespace BookingPlatform.BookingService.Application.Bookings.GetMy;

public sealed class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, IReadOnlyList<BookingResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;

    public GetMyBookingsQueryHandler(IBookingRepository bookingRepository, ICurrentUser currentUser)
    {
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BookingResponse>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        if (request.From.HasValue && request.To.HasValue && request.To <= request.From)
            throw new ArgumentException("Конец периода должен быть больше начала.");

        var bookings = await _bookingRepository.GetByUserAsync(
            _currentUser.UserId,
            request.Status,
            request.From,
            request.To,
            cancellationToken);

        return bookings.Select(x => x.ToResponse()).ToList();
    }
}
