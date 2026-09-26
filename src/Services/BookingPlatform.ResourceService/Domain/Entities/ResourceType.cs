namespace BookingPlatform.ResourceService.Domain.Entities;

public sealed class ResourceType
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /*конструктор для создания ResourceType из MongoDB. Для упрощения логики не стал разделять*/
    private ResourceType()
    {
        // Для MongoDB
    }

    public ResourceType(Guid id, string name, string? description = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Идентификатор типа ресурса не может быть пустым.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя типа ресурса не может быть пустым.", nameof(name));

        Id = id;
        Name = name.Trim();
        Description = description?.Trim();
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя resource type не может быть пустым.", nameof(name));

        Name = name.Trim();
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }

    public void Activate()
    {
        IsActive = true;
    }
/*Деактивируем тип вместо физического удаления */
    public void Deactivate()
    {
        IsActive = false;
    }
}