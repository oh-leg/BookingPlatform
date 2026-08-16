namespace BookingPlatform.ResourceService.Domain.Entities;

public sealed class ResourceOffer
{
    private readonly List<ResourceSchedule> _schedule = [];
    public IReadOnlyCollection<ResourceSchedule> Schedule => _schedule.AsReadOnly();
    /*конструктор для создания ResourceOffer из MongoDB*/
    private ResourceOffer()
    {
        // Для MongoDB
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public int DurationMinutes { get; private set; }

    public decimal Price { get; private set; }

    public string Currency { get; private set; } = null!;

    public int BufferBeforeMinutes { get; private set; }

    public int BufferAfterMinutes { get; private set; }

    public int CancellationDeadlineHours { get; private set; }

    public bool RequiresConfirmation { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public ResourceOffer(
        Guid id,
        string name,
        int durationMinutes,
        decimal price,
        string currency,
        int bufferBeforeMinutes = 0,
        int bufferAfterMinutes = 0,
        int cancellationDeadlineHours = 0,
        bool requiresConfirmation = false,
        string? description = null
        )
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Идентификатор услуги не может быть пустым.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Наименование услуги не может быть пустым.", nameof(name));

        if (durationMinutes <= 0)
            throw new ArgumentException("Время выполнения не может быть меньше нуля.", nameof(durationMinutes));

        if (price < 0)
            throw new ArgumentException("Цена не может быть отрицательной.", nameof(price));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Валюта не может быть пустой.", nameof(currency));

        if (bufferBeforeMinutes < 0)
            throw new ArgumentException("Время подготовки перед оказанием услуги не может быть отрицательным.", nameof(bufferBeforeMinutes));

        if (bufferAfterMinutes < 0)
            throw new ArgumentException("Время подготовки после оказания услуги не может быть отрицательным.", nameof(bufferAfterMinutes));

        if (cancellationDeadlineHours < 0)
            throw new ArgumentException("Срок отказа от услуги не может быть отрицательным.", nameof(cancellationDeadlineHours));

        Id = id;
        Name = name.Trim();
        Description = description?.Trim();
        DurationMinutes = durationMinutes;
        Price = price;
        Currency = currency.Trim().ToUpperInvariant();
        BufferBeforeMinutes = bufferBeforeMinutes;
        BufferAfterMinutes = bufferAfterMinutes;
        CancellationDeadlineHours = cancellationDeadlineHours;
        RequiresConfirmation = requiresConfirmation;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Наименование услуги не может быть пустым.", nameof(name));

        Name = name.Trim();
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }

    public void ChangeDuration(int durationMinutes)
    {
        if (durationMinutes <= 0)
            throw new ArgumentException("Время выполнения не может быть отрицательной.", nameof(durationMinutes));

        DurationMinutes = durationMinutes;
    }

    public void ChangePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Цена не может быть отрицательной.", nameof(price));

        Price = price;
    }

    public void ChangeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Валюта не может быть пустой.", nameof(currency));

        Currency = currency.Trim().ToUpperInvariant();
    }

    public void ChangeBuffers(int bufferBeforeMinutes, int bufferAfterMinutes)
    {
        if (bufferBeforeMinutes < 0)
            throw new ArgumentException("Время подготовки перед оказанием услуги не может быть отрицательным.", nameof(bufferBeforeMinutes));

        if (bufferAfterMinutes < 0)
            throw new ArgumentException("Время подготовки после оказания услуги не может быть отрицательным.", nameof(bufferAfterMinutes));

        BufferBeforeMinutes = bufferBeforeMinutes;
        BufferAfterMinutes = bufferAfterMinutes;
    }

    public void ChangeCancellationDeadline(int hours)
    {
        if (hours < 0)
            throw new ArgumentException("Срок отказа от услуги не может быть отрицательным.", nameof(hours));

        CancellationDeadlineHours = hours;
    }

    public void SetConfirmationRequired(bool required)
    {
        RequiresConfirmation = required;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
/*Добавляем расписание*/
    public void AddSchedule(ResourceSchedule schedule)
    {
        if (schedule is null)
            throw new ArgumentNullException(nameof(schedule), "Нужно указать интервал.");
        
        if (_schedule.Any(x =>
                x.IsActive &&
                x.DayOfWeek == schedule.DayOfWeek &&
                schedule.StartTime < x.EndTime &&
                schedule.EndTime > x.StartTime))
        {
            throw new InvalidOperationException("Указанный интервал расписания пересекается с уже существующим.");
        }
        
        _schedule.Add(schedule);
    }
}