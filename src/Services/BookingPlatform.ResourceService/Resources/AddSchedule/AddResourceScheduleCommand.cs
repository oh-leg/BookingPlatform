using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.AddSchedule;

public sealed record AddResourceScheduleCommand(Guid ResourceId, Guid OfferId, DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime) : IRequest<Guid?>;
