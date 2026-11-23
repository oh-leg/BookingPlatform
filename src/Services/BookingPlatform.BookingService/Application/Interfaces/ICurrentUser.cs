namespace BookingPlatform.BookingService.Application.Interfaces;

public interface ICurrentUser
{
    string UserId { get; }

    /* Имеет ли пользователь роль booking-admin */
    bool IsAdmin { get; }
}
