namespace BookingPlatform.ResourceService.API.Contracts.Resources;

public sealed record AddResourceScheduleRequest(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
