using BookingPlatform.ResourceService.Application.Interfaces;
using BookingPlatform.ResourceService.Domain.Entities;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.AddScheduleOverride;

public sealed class AddScheduleOverrideCommandHandler : IRequestHandler<AddScheduleOverrideCommand, Guid?>
{
    private readonly IResourceRepository _resourceRepository;

    public AddScheduleOverrideCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<Guid?> Handle(AddScheduleOverrideCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);

        if (resource is null)
            return null;

        var scheduleOverride = new ResourceScheduleOverride(Guid.NewGuid(), resource.Id, request.StartAt, request.EndAt, request.Type, request.Reason);

        resource.AddScheduleOverride(scheduleOverride);

        await _resourceRepository.UpdateAsync(resource, cancellationToken);

        return scheduleOverride.Id;
    }
}
