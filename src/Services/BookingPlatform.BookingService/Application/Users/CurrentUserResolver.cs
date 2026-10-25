using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Entities;

namespace BookingPlatform.BookingService.Application.Users;

/*Находит локальный профиль по sub из токена, при первом обращении создаёт его из claims*/
public sealed class CurrentUserResolver
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _users;

    public CurrentUserResolver(ICurrentUser currentUser, IUserRepository users)
    {
        _currentUser = currentUser;
        _users = users;
    }

    public Task<User> ResolveAsync(CancellationToken cancellationToken)
    {
        var candidate = new User(_currentUser.KeycloakId, _currentUser.Email, _currentUser.FullName, _currentUser.Phone);

        return _users.GetOrCreateAsync(candidate, cancellationToken);
    }
}
