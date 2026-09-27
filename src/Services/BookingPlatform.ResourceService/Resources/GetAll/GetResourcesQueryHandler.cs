using BookingPlatform.ResourceService.Application.Interfaces;
using BookingPlatform.ResourceService.Application.Resources.GetById;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.GetAll;

public sealed class GetResourcesQueryHandler : IRequestHandler<GetResourcesQuery, IReadOnlyCollection<ResourceResponse>>
{
    private readonly IResourceRepository _resourceRepository;

    public GetResourcesQueryHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<IReadOnlyCollection<ResourceResponse>> Handle(GetResourcesQuery request, CancellationToken cancellationToken)
    {
        var resources = await _resourceRepository.GetAllAsync(cancellationToken);
        return resources.Select(r => r.ToResponse()).ToList().AsReadOnly();
    }
}
