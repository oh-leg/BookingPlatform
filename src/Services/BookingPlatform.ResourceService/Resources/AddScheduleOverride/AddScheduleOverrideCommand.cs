using BookingPlatform.ResourceService.Domain.Enums;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.AddScheduleOverride;

public sealed record AddScheduleOverrideCommand(Guid ResourceId, DateTimeOffset StartAt, DateTimeOffset EndAt, ScheduleOverrideType Type, string? Reason) : IRequest<Guid?>;
