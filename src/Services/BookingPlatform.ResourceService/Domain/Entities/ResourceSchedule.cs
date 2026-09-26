namespace BookingPlatform.ResourceService.Domain.Entities;

public sealed class ResourceSchedule
{
    /*конструктор для создания ResourceSchedule из MongoDB*/
    private ResourceSchedule()
    {
        // Для MongoDB
    }

    public Guid Id { get; private set; }

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public bool IsActive { get; private set; }

    public ResourceSchedule(Guid id, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Идентификатор расписание не может быть пустым.", nameof(id));

        if (endTime <= startTime)
            throw new ArgumentException("Время окончания расписания должна быть больше времени начала.", nameof(endTime));

        Id = id;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        IsActive = true;
    }

    public void Update(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("Время окончания расписания должна быть больше времени начала.", nameof(endTime));

        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
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