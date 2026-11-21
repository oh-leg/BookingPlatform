using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace BookingPlatform.BookingService.API.Auth;

/* Преобразование ролей из JWT-токена Keycloak в стандартные роли. Используя встроенную авторизацию 
 * Нужно реализовать в ResourceService преобразование ролей
 */
public sealed class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    private const string RealmAccessClaim = "realm_access";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identity)
            return Task.FromResult(principal);

        var realmAccess = identity.FindFirst(RealmAccessClaim)?.Value;

        if (string.IsNullOrWhiteSpace(realmAccess))
            return Task.FromResult(principal);

        foreach (var role in ParseRoles(realmAccess))
        {
            if (!identity.HasClaim(identity.RoleClaimType, role))
                identity.AddClaim(new Claim(identity.RoleClaimType, role));
        }

        return Task.FromResult(principal);
    }

    internal static IEnumerable<string> ParseRoles(string realmAccessJson)
    {
        try
        {
            using var document = JsonDocument.Parse(realmAccessJson);

            if (!document.RootElement.TryGetProperty("roles", out var roles) || roles.ValueKind != JsonValueKind.Array)
                return [];

            return roles.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => x.GetString()!)
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
