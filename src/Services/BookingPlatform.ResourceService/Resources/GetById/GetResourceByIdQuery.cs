using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.GetById;

public sealed record GetResourceByIdQuery(Guid Id) : IRequest<ResourceResponse?>;