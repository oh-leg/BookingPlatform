using BookingPlatform.ResourceService.Application.Interfaces;
using BookingPlatform.ResourceService.Domain.Entities;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.Create;

public sealed class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, Guid>
{
    private readonly IResourceRepository _resourceRepository;

    public CreateResourceCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<Guid> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        var resourceType = new ResourceType(Guid.NewGuid(), request.ResourceTypeName, request.ResourceTypeDescription);

        var resource = new Resource(Guid.NewGuid(), resourceType, request.Name, request.Description, request.Location, request.Capacity, request.Metadata);

        await _resourceRepository.AddAsync(resource, cancellationToken);

        return resource.Id;
    }
}