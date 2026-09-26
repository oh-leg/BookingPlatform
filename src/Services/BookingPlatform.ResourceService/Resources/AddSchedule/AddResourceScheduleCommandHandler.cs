using BookingPlatform.ResourceService.Application.Interfaces;
using BookingPlatform.ResourceService.Domain.Entities;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.AddSchedule;

public sealed class AddResourceScheduleCommandHandler : IRequestHandler<AddResourceScheduleCommand, Guid?>
{
    private readonly IResourceRepository _resourceRepository;

    public AddResourceScheduleCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<Guid?> Handle(AddResourceScheduleCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);

        if (resource is null)
            return null;

        if (!resource.Offers.Any(o => o.Id == request.OfferId))
            return null;

        var schedule = new ResourceSchedule(Guid.NewGuid(), request.DayOfWeek, request.StartTime, request.EndTime);

        resource.AddOfferSchedule(request.OfferId, schedule);

        await _resourceRepository.UpdateAsync(resource, cancellationToken);

        return schedule.Id;
    }
}
