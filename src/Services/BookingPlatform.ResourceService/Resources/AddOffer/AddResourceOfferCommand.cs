using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.AddOffer;

public sealed record AddResourceOfferCommand(Guid ResourceId, string Name, string? Description, int DurationMinutes, decimal Price, string Currency, int BufferBeforeMinutes, int BufferAfterMinutes, int CancellationDeadlineHours, bool RequiresConfirmation) : IRequest<Guid?>;
