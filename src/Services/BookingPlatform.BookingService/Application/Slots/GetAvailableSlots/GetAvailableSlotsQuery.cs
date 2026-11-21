using MediatR;

namespace BookingPlatform.BookingService.Application.Slots.GetAvailableSlots;

public sealed record GetAvailableSlotsQuery(Guid ResourceId, Guid OfferId, DateOnly From, DateOnly To)
    : IRequest<IReadOnlyList<SlotResponse>>;

public sealed record SlotResponse(DateTimeOffset StartAt, DateTimeOffset EndAt);
