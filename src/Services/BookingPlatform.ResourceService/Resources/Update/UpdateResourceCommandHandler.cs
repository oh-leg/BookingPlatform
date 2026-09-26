using BookingPlatform.ResourceService.Application.Interfaces;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.Update;

public sealed class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand, bool>
{
    private readonly IResourceRepository _resourceRepository;

    public UpdateResourceCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<bool> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);

        if (resource is null)
            return false;

        resource.Rename(request.Name);
        resource.UpdateDescription(request.Description);
        resource.UpdateLocation(request.Location);
        resource.UpdateCapacity(request.Capacity);
        resource.UpdateMetadata(request.Metadata);

        await _resourceRepository.UpdateAsync(resource, cancellationToken);

        return true;
    }
}
