namespace BookingPlatform.ResourceService.Domain.Entities;

public sealed class Resource
{
    private List<ResourceOffer> _offers = [];
    private List<ResourceScheduleOverride> _scheduleOverrides = [];

    /* Конструктор для создания Resource из MongoDB */
    private Resource()
    {
        // Для MongoDB
    }

    public Guid Id { get; private set; }

    public ResourceType ResourceType { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? Location { get; private set; }

    public int? Capacity { get; private set; }

    public Dictionary<string, string>? Metadata { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<ResourceOffer> Offers => _offers.AsReadOnly();

    public IReadOnlyCollection<ResourceScheduleOverride> ScheduleOverrides => _scheduleOverrides.AsReadOnly();

    public Resource(
        Guid id,
        ResourceType resourceType,
        string name,
        string? description = null,
        string? location = null,
        int? capacity = null,
        Dictionary<string, string>? metadata = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Идентификатор ресурса не может быть пустым.", nameof(id));

        if (resourceType is null)
            throw new ArgumentException("Идентификатор типа ресурса не может быть пустым.", nameof(resourceType));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Наименование ресурса не может быть пустым.", nameof(name));

        if (capacity.HasValue && capacity <= 0)
            throw new ArgumentException("Емкость ресурса должна быть больше нуля.", nameof(capacity));

        Id = id;
        ResourceType = resourceType;
        Name = name.Trim();
        Description = description?.Trim();
        Location = location?.Trim();
        Capacity = capacity;
        Metadata = metadata;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Наименование ресурса не может быть пустым.", nameof(name));

        Name = name.Trim();
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }

    public void UpdateLocation(string? location)
    {
        Location = location?.Trim();
    }

    public void UpdateCapacity(int? capacity)
    {
        if (capacity.HasValue && capacity <= 0)
            throw new ArgumentException("Емкость ресурса должна быть больше нуля.", nameof(capacity));

        Capacity = capacity;
    }

    public void UpdateMetadata(Dictionary<string, string>? metadata)
    {
        Metadata = metadata;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
/*Добавление интервала в расписание услуги с проверкой на пересечение*/
    public void AddOffer(ResourceOffer offer)
    {
        if (offer is null)
            throw new ArgumentNullException(nameof(offer), "Необходимо указать услугу.");

        if (_offers.Any(x => x.IsActive && string.Equals(x.Name, offer.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("У ресурса уже существует услуга с таким названием.");
        }

        _offers.Add(offer);
    }
    public void AddOfferSchedule(Guid offerId, ResourceSchedule schedule)
    {
        var offer = _offers.FirstOrDefault(x => x.Id == offerId)
            ?? throw new InvalidOperationException("Услуга не найдена.");

        offer.AddSchedule(schedule);
    }

/*Добавление исключения расписания услуги с проверкой на пересечение*/
    public void AddScheduleOverride(ResourceScheduleOverride scheduleOverride)
    {
        if (scheduleOverride is null)
            throw new ArgumentNullException(nameof(scheduleOverride), "Необходимо указать исключение расписания.");

        if (scheduleOverride.ResourceId != Id)
            throw new InvalidOperationException("Исключение расписания относится к другому ресурсу.");

        if (_scheduleOverrides.Any(x =>
                x.IsActive &&
                scheduleOverride.StartAt < x.EndAt &&
                scheduleOverride.EndAt > x.StartAt))
        {
            throw new InvalidOperationException("Период исключения расписания пересекается с уже существующим.");
        }

        _scheduleOverrides.Add(scheduleOverride);
    }
}
