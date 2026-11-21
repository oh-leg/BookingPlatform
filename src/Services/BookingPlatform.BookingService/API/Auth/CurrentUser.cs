using System.Security.Claims;
using BookingPlatform.BookingService.Application.Interfaces;

namespace BookingPlatform.BookingService.API.Auth;

public static class BookingRoles
{
    public const string Admin = "booking-admin";

    public const string AdminPolicy = "BookingAdmin";
}

/* Получаем пользователя из HttpContext*/
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal Principal =>_httpContextAccessor.HttpContext?.User ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован.");

    public string UserId =>Principal.FindFirstValue("sub") ?? Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("В токене отсутствует идентификатор пользователя.");

    public bool IsAdmin => Principal.IsInRole(BookingRoles.Admin);
}
