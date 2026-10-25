namespace BookingPlatform.BookingService.Application.Interfaces;

/*Данные текущего пользователя из JWT*/
public interface ICurrentUser
{
    Guid KeycloakId { get; }

    string Email { get; }

    string FullName { get; }

    string? Phone { get; }

    bool IsManager { get; }
}
