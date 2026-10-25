using BookingPlatform.BookingService.Application.Interfaces;
using MediatR;

namespace BookingPlatform.BookingService.Bookings.GetAll;

public sealed class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, IReadOnlyList<BookingResponse>>
{
    private readonly IBookingRepository _bookings;
    private readonly ICurrentUser _currentUser;

    public GetAllBookingsQueryHandler(IBookingRepository bookings, ICurrentUser currentUser)
    {
        _bookings = bookings;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BookingResponse>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsManager)
            throw new UnauthorizedAccessException("Действие доступно только менеджеру.");

        var bookings = await _bookings.GetListAsync(
            new BookingFilter(null, request.ResourceId, request.Status, request.From, request.To, request.Page, request.PageSize),
            cancellationToken);

        return bookings.Select(BookingResponse.From).ToList();
    }
}
