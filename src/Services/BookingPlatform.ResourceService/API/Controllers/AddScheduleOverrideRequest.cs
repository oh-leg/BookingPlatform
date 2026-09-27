using BookingPlatform.ResourceService.Domain.Enums;

namespace BookingPlatform.ResourceService.API.Contracts.Resources;

public sealed record AddScheduleOverrideRequest(DateTimeOffset StartAt, DateTimeOffset EndAt, ScheduleOverrideType Type, string? Reason);
