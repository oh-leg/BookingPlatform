using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.Deactivate;

public sealed record DeactivateResourceCommand(Guid ResourceId) : IRequest<bool>;
