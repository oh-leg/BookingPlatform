using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.Activate;

public sealed record ActivateResourceCommand(Guid ResourceId) : IRequest<bool>;
