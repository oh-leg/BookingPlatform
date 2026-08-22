using BookingPlatform.ResourceService.Domain.Enums;

namespace BookingPlatform.ResourceService.Domain.Entities;

public sealed class ResourceScheduleOverride
{
    /* Конструктор для создания ResourceScheduleOverride из MongoDB */
    private ResourceScheduleOverride()
    {
        // Для MongoDB
    }

    public Guid Id { get; private set; }
    
    public Guid ResourceId { get; private set; }

    public DateTimeOffset StartAt { get; private set; }

    public DateTimeOffset EndAt { get; private set; }

    public ScheduleOverrideType Type { get; private set; }

    public string? Reason { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public ResourceScheduleOverride(
        Guid id,
        Guid resourceId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        ScheduleOverrideType type,
        string? reason = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Идентификатор исключения расписания не может быть пустым.", nameof(id));
        
        if (resourceId == Guid.Empty)
            throw new ArgumentException("Идентификатор ресурса не может быть пустым", nameof(resourceId));
        
        if (endAt <= startAt)
            throw new ArgumentException("Время окончания должно быть больше времени начала.", nameof(endAt));

        if (!Enum.IsDefined(type))
            throw new ArgumentException("Указан некорректный тип исключения расписания.", nameof(type));

        Id = id;
        ResourceId = resourceId;
        StartAt = startAt;
        EndAt = endAt;
        Type = type;
        Reason = reason?.Trim();
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        ScheduleOverrideType type,
        string? reason = null)
    {
        if (endAt <= startAt)
            throw new ArgumentException("Время окончания должно быть больше времени начала.", nameof(endAt));

        if (!Enum.IsDefined(type))
            throw new ArgumentException("Указан некорректный тип исключения расписания.", nameof(type));

        StartAt = startAt;
        EndAt = endAt;
        Type = type;
        Reason = reason?.Trim();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}