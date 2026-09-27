using BookingPlatform.ResourceService.Application.Interfaces;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.Deactivate;

public sealed class DeactivateResourceCommandHandler : IRequestHandler<DeactivateResourceCommand, bool>
{
    private readonly IResourceRepository _resourceRepository;

    public DeactivateResourceCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<bool> Handle(DeactivateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);

        if (resource is null)
            return false;

        resource.Deactivate();

        await _resourceRepository.UpdateAsync(resource, cancellationToken);

        return true;
    }
}
