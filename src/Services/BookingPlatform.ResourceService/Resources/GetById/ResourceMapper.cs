using BookingPlatform.ResourceService.Domain.Entities;

namespace BookingPlatform.ResourceService.Application.Resources.GetById;

internal static class ResourceMapper
{
    internal static ResourceResponse ToResponse(this Resource resource) =>
        new(
            resource.Id,
            new ResourceTypeResponse(
                resource.ResourceType.Id,
                resource.ResourceType.Name,
                resource.ResourceType.Description,
                resource.ResourceType.IsActive,
                resource.ResourceType.CreatedAt),
            resource.Name,
            resource.Description,
            resource.Location,
            resource.Capacity,
            resource.Metadata,
            resource.IsActive,
            resource.CreatedAt,
            resource.Offers.Select(o => new ResourceOfferResponse(
                o.Id,
                o.Name,
                o.Description,
                o.DurationMinutes,
                o.Price,
                o.Currency,
                o.BufferBeforeMinutes,
                o.BufferAfterMinutes,
                o.CancellationDeadlineHours,
                o.RequiresConfirmation,
                o.IsActive,
                o.CreatedAt,
                o.Schedule.Select(s => new ResourceScheduleResponse(
                    s.Id,
                    s.DayOfWeek,
                    s.StartTime,
                    s.EndTime,
                    s.IsActive)).ToList().AsReadOnly())).ToList().AsReadOnly(),
            resource.ScheduleOverrides.Select(so => new ResourceScheduleOverrideResponse(
                so.Id,
                so.StartAt,
                so.EndAt,
                so.Type,
                so.Reason,
                so.IsActive,
                so.CreatedAt)).ToList().AsReadOnly());
}
