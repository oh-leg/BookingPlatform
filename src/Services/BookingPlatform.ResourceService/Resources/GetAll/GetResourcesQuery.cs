using BookingPlatform.ResourceService.Application.Resources.GetById;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.GetAll;

public sealed record GetResourcesQuery : IRequest<IReadOnlyCollection<ResourceResponse>>;
