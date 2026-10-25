using BookingPlatform.BookingService.Domain.Entities;

namespace BookingPlatform.BookingService.Application.Interfaces;

public interface IUserRepository
{
    /*Возвращает профиль по keycloak_id, при отсутствии сохраняет переданный*/
    Task<User> GetOrCreateAsync(User candidate, CancellationToken cancellationToken);
}
