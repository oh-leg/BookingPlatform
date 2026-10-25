using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Application.Users;
using BookingPlatform.BookingService.Domain.Entities;
using MediatR;

namespace BookingPlatform.BookingService.Bookings.Create;

public sealed class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingResponse>
{
    private readonly IBookingRepository _bookings;
    private readonly CurrentUserResolver _userResolver;

    public CreateBookingCommandHandler(IBookingRepository bookings, CurrentUserResolver userResolver)
    {
        _bookings = bookings;
        _userResolver = userResolver;
    }

    public async Task<BookingResponse> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var user = await _userResolver.ResolveAsync(cancellationToken);

        // пустые данные клиента берём из профиля
        var booking = new Booking(
            user.Id,
            request.ResourceId,
            request.OfferId,
            request.StartTime,
            request.Offer,
            string.IsNullOrWhiteSpace(request.ClientName) ? user.FullName : request.ClientName,
            string.IsNullOrWhiteSpace(request.ClientEmail) ? user.Email : request.ClientEmail,
            string.IsNullOrWhiteSpace(request.ClientPhone) ? user.Phone : request.ClientPhone,
            request.Comment,
            DateTimeOffset.UtcNow);

        await _bookings.AddAsync(booking, cancellationToken);

        return BookingResponse.From(booking);
    }
}
