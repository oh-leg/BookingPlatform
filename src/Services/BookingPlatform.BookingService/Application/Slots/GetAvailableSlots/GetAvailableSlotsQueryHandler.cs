using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Exceptions;
using BookingPlatform.BookingService.Domain.Slots;
using MediatR;
using Microsoft.Extensions.Options;

namespace BookingPlatform.BookingService.Application.Slots.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<SlotResponse>>
{
    private readonly IResourceCatalogClient _resourceCatalog;
    private readonly IBookingRepository _bookingRepository;
    private readonly TimeProvider _timeProvider;
    private readonly BookingOptions _options;

    public GetAvailableSlotsQueryHandler(
        IResourceCatalogClient resourceCatalog,
        IBookingRepository bookingRepository,
        TimeProvider timeProvider,
        IOptions<BookingOptions> options)
    {
        _resourceCatalog = resourceCatalog;
        _bookingRepository = bookingRepository;
        _timeProvider = timeProvider;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<SlotResponse>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        if (request.To < request.From)
            throw new ArgumentException("Дата окончания периода должна быть не раньше даты начала.");

        if (request.To.DayNumber - request.From.DayNumber + 1 > _options.MaxSlotsRangeDays)
            throw new ArgumentException($"Период не может превышать {_options.MaxSlotsRangeDays} дн.");

        var resource = await _resourceCatalog.GetResourceAsync(request.ResourceId, cancellationToken)
            ?? throw new ResourceNotFoundException("Ресурс не найден.");

        if (resource.FindOffer(request.OfferId) is null)
            throw new ResourceNotFoundException("Услуга не найдена.");

        var slots = await SlotAvailability.CalculateAsync(
            resource,
            request.OfferId,
            request.From,
            request.To,
            _bookingRepository,
            _options,
            _timeProvider.GetUtcNow(),
            cancellationToken);

        return slots.Select(x => new SlotResponse(x.Start, x.End)).ToList();
    }
}
