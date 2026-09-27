using BookingPlatform.ResourceService.Domain.Enums;

namespace BookingPlatform.ResourceService.Application.Resources.GetById;

public sealed record ResourceResponse(
    Guid Id,
    ResourceTypeResponse ResourceType,
    string Name,
    string? Description,
    string? Location,
    int? Capacity,
    Dictionary<string, string>? Metadata,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<ResourceOfferResponse> Offers,
    IReadOnlyCollection<ResourceScheduleOverrideResponse> ScheduleOverrides);

public sealed record ResourceTypeResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record ResourceOfferResponse(
    Guid Id,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Price,
    string Currency,
    int BufferBeforeMinutes,
    int BufferAfterMinutes,
    int CancellationDeadlineHours,
    bool RequiresConfirmation,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<ResourceScheduleResponse> Schedule);

public sealed record ResourceScheduleResponse(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);

public sealed record ResourceScheduleOverrideResponse(
    Guid Id,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    ScheduleOverrideType Type,
    string? Reason,
    bool IsActive,
    DateTimeOffset CreatedAt);
