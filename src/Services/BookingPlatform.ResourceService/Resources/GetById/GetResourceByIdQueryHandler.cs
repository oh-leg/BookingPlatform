using BookingPlatform.ResourceService.Application.Interfaces;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.GetById;

public sealed class GetResourceByIdQueryHandler : IRequestHandler<GetResourceByIdQuery, ResourceResponse?>
{
    private readonly IResourceRepository _resourceRepository;

    public GetResourceByIdQueryHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<ResourceResponse?> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.Id, cancellationToken);

        if (resource is null)
            return null;

        return resource.ToResponse();
    }
}
