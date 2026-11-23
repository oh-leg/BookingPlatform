using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Options;

namespace BookingPlatform.BookingService.Application.Bookings.GetByResource;

public sealed class GetResourceBookingsQueryHandler : IRequestHandler<GetResourceBookingsQuery, IReadOnlyList<BookingResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;
    private readonly BookingOptions _options;

    public GetResourceBookingsQueryHandler(
        IBookingRepository bookingRepository,
        ICurrentUser currentUser,
        IOptions<BookingOptions> options)
    {
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<BookingResponse>> Handle(GetResourceBookingsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
            throw new ForbiddenBookingActionException("Просматривать брони ресурса может только администратор.");

        if (request.To <= request.From)
            throw new ArgumentException("Конец периода должен быть больше начала.");

        if (request.To - request.From > TimeSpan.FromDays(_options.MaxBookingsRangeDays))
            throw new ArgumentException($"Период не может превышать {_options.MaxBookingsRangeDays} дн.");

        var bookings = await _bookingRepository.GetByResourceAsync(
            request.ResourceId,
            request.From,
            request.To,
            cancellationToken);

        return bookings.Select(x => x.ToResponse()).ToList();
    }
}
