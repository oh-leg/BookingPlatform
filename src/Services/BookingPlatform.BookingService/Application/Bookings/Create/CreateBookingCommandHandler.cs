using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Application.Slots;
using BookingPlatform.BookingService.Domain.Entities;
using BookingPlatform.BookingService.Domain.Exceptions;
using BookingPlatform.BookingService.Domain.Slots;
using MediatR;
using Microsoft.Extensions.Options;

namespace BookingPlatform.BookingService.Application.Bookings.Create;

public sealed class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingResponse>
{
    private readonly IResourceCatalogClient _resourceCatalog;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly BookingOptions _options;

    public CreateBookingCommandHandler(
        IResourceCatalogClient resourceCatalog,
        IBookingRepository bookingRepository,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        IOptions<BookingOptions> options)
    {
        _resourceCatalog = resourceCatalog;
        _bookingRepository = bookingRepository;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _options = options.Value;
    }

    public async Task<BookingResponse> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();

        if (request.StartAt <= now)
            throw new ArgumentException("Нельзя забронировать время в прошлом.");

        var resource = await _resourceCatalog.GetResourceAsync(request.ResourceId, cancellationToken)
            ?? throw new ResourceNotFoundException("Ресурс не найден.");

        var offer = resource.FindOffer(request.OfferId)
            ?? throw new ResourceNotFoundException("Услуга не найдена.");

        if (!resource.IsActive || !offer.IsActive)
            throw new InvalidOperationException("Ресурс или услуга недоступны для бронирования.");

        // Проверяем, что запрошенное время совпадает с одним из свободных слотов дня.
        // Окончательную защиту от гонок обеспечивает exclusion constraint в PostgreSQL(ограничение которое запрещает существование дву строк конфликтующих по определенному условию).
        var localDate = SlotCalculator.ToLocalDate(request.StartAt, _options.GetTimeZone());

        var slots = await SlotAvailability.CalculateAsync(
            resource,
            offer.Id,
            localDate,
            localDate,
            _bookingRepository,
            _options,
            now,
            cancellationToken);

        var requestedStart = request.StartAt.ToUniversalTime();

        if (!slots.Any(x => x.Start == requestedStart))
            throw new BookingConflictException("Выбранное время недоступно для бронирования.");

        var booking = Booking.Create(
            Guid.NewGuid(),
            _currentUser.UserId,
            resource.Id,
            offer.ToBookedOffer(),
            requestedStart,
            now,
            request.Comment);

        await _bookingRepository.AddAsync(booking, cancellationToken);

        return booking.ToResponse();
    }
}
