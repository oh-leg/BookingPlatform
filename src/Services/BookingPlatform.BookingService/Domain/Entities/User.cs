namespace BookingPlatform.BookingService.Domain.Entities;

/*Локальный профиль пользователя. Пароли и роли хранит Keycloak*/
public sealed class User
{
    private User()
    {
        // Для Dapper
    }

    public Guid Id { get; private set; }

    public Guid KeycloakId { get; private set; }

    public string Email { get; private set; } = null!;

    public string FullName { get; private set; } = null!;

    public string? Phone { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public User(Guid keycloakId, string email, string fullName, string? phone = null)
    {
        if (keycloakId == Guid.Empty)
            throw new ArgumentException("Идентификатор Keycloak не может быть пустым.", nameof(keycloakId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не может быть пустым.", nameof(email));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Имя не может быть пустым.", nameof(fullName));

        Id = Guid.NewGuid();
        KeycloakId = keycloakId;
        Email = email.Trim();
        FullName = fullName.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
    }
}
