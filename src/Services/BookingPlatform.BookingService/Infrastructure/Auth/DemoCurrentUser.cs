using BookingPlatform.BookingService.Application.Interfaces;

namespace BookingPlatform.BookingService.Infrastructure.Auth;

/*Фиксированный пользователь для показа функционала без авторизации.
  Менеджер, чтобы были доступны все операции. Вернуть CurrentUser, когда понадобится JWT*/
public sealed class DemoCurrentUser : ICurrentUser
{
    public Guid KeycloakId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public string Email => "demo@bookingplatform.local";

    public string FullName => "Demo User";

    public string? Phone => null;

    public bool IsManager => true;
}
