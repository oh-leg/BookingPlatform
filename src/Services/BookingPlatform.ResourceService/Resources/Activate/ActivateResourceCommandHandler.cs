using BookingPlatform.ResourceService.Application.Interfaces;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.Activate;

public sealed class ActivateResourceCommandHandler : IRequestHandler<ActivateResourceCommand, bool>
{
    private readonly IResourceRepository _resourceRepository;

    public ActivateResourceCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<bool> Handle(ActivateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);

        if (resource is null)
            return false;

        resource.Activate();

        await _resourceRepository.UpdateAsync(resource, cancellationToken);

        return true;
    }
}
