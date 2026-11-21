namespace BookingPlatform.BookingService.Infrastructure.ResourceCatalog.Dto;

/* Контракт ответа от ResourceService GET api/resources/{id} */
public sealed record ResourceDto(
    Guid Id,
    bool IsActive,
    IReadOnlyCollection<OfferDto>? Offers,
    IReadOnlyCollection<ScheduleOverrideDto>? ScheduleOverrides);

public sealed record OfferDto(
    Guid Id,
    string Name,
    int DurationMinutes,
    decimal Price,
    string Currency,
    int BufferBeforeMinutes,
    int BufferAfterMinutes,
    int CancellationDeadlineHours,
    bool RequiresConfirmation,
    bool IsActive,
    IReadOnlyCollection<ScheduleDto>? Schedule);

public sealed record ScheduleDto(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);

public sealed record ScheduleOverrideDto(
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    int Type,
    bool IsActive);
