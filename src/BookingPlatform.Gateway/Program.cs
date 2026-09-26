using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

// Временно
// KEYCLOAK_HOSTNAME — внутренний адрес Keycloak (для metadata из кластера).
// KEYCLOAK_ISSUER — публичный issuer токенов (см. KC_HOSTNAME у Keycloak); по умолчанию равен внутреннему.
// KEYCLOAK_JWKS — внутренний jwks-эндпоинт Keycloak. Если задан, ключи подписи загружаются напрямую с него,
//   а issuer токенов валидируется как публичный (http://localhost:8080/auth/...), недоступный изнутри кластера.
var keycloakHost = builder.Configuration["KEYCLOAK_HOSTNAME"] ?? "localhost";
var internalAuthority = $"http://{keycloakHost}:8080/realms/booking-platform";
var publicIssuer = builder.Configuration["KEYCLOAK_ISSUER"] ?? internalAuthority;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Audience = "booking-api";
        options.RequireHttpsMetadata = false;

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = publicIssuer
        };

        var internalJwks = builder.Configuration["KEYCLOAK_JWKS"];
        if (!string.IsNullOrEmpty(internalJwks))
        {
            // Шлюз в кластере не может ходить на публичный issuer (localhost:8080), поэтому
            // ключи берём с внутреннего jwks Keycloak, кэшируя их до смены kid.
            var jwksHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            IReadOnlyCollection<SecurityKey>? signingKeys = null;
            tokenValidationParameters.IssuerSigningKeyResolver = (_, _, kid, _) =>
            {
                var keys = Volatile.Read(ref signingKeys);
                if (keys == null || (kid != null && keys.All(k => k.KeyId != kid)))
                {
                    var jwksJson = jwksHttp.GetStringAsync(internalJwks).GetAwaiter().GetResult();
                    keys = new JsonWebKeySet(jwksJson).GetSigningKeys().ToList();
                    Volatile.Write(ref signingKeys, keys);
                }
                return keys;
            };
        }
        else
        {
            options.Authority = internalAuthority;
        }

        options.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.AddHttpClient();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
});

builder.Services.AddAuthorization();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseHttpLogging();

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy()
    .RequireAuthorization();

app.Run();
