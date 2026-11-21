using System.Text.Json.Serialization;
using BookingPlatform.BookingService;
using BookingPlatform.BookingService.API.Auth;
using BookingPlatform.BookingService.API.ExceptionHandling;
using BookingPlatform.BookingService.Application.Bookings.Create;
using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Infrastructure.Persistence;
using BookingPlatform.BookingService.Infrastructure.Persistence.Migrations;
using BookingPlatform.BookingService.Infrastructure.ResourceCatalog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Настраиваю сервис
builder.Services.AddOptions<BookingOptions>()
    .Bind(builder.Configuration.GetSection(BookingOptions.SectionName))
    .Validate(o => o.SlotStepMinutes > 0, "Booking:SlotStepMinutes должен быть больше нуля.")
    .ValidateOnStart();
builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection(PostgresOptions.SectionName));
builder.Services.Configure<ResourceCatalogOptions>(builder.Configuration.GetSection(ResourceCatalogOptions.SectionName));

// PostgreSQL
builder.Services.AddSingleton(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString(PostgresOptions.ConnectionStringName);

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException($"Не задана строка подключения ConnectionStrings:{PostgresOptions.ConnectionStringName}.");

    return NpgsqlDataSource.Create(connectionString);
});
builder.Services.AddSingleton<IBookingRepository, BookingRepository>();
builder.Services.AddSingleton<DatabaseMigrator>();
builder.Services.AddHostedService<DatabaseMigrationHostedService>();

// Обращение к ResourceService
builder.Services.AddHttpClient<IResourceCatalogClient, ResourceCatalogClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ResourceCatalogOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblyContaining<CreateBookingCommand>());

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Аутентификация: JWT Keycloak (взял настройки из Gateway).
// KEYCLOAK_HOSTNAME — local адрес Keycloak, KEYCLOAK_ISSUER — публичный issuer,
// KEYCLOAK_JWKS — внутренний jwks-эндпоинт (если issuer недоступен изнутри сети).
var keycloakHost = builder.Configuration["KEYCLOAK_HOSTNAME"] ?? "localhost";
var internalAuthority = $"http://{keycloakHost}:8080/realms/booking-platform";
var publicIssuer = builder.Configuration["KEYCLOAK_ISSUER"] ?? internalAuthority;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Audience = "booking-api";
        options.RequireHttpsMetadata = false;        
        options.MapInboundClaims = false;

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = publicIssuer,
            NameClaimType = "preferred_username"
        };

        var internalJwks = builder.Configuration["KEYCLOAK_JWKS"];
        if (!string.IsNullOrEmpty(internalJwks))
        {
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

builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(BookingRoles.AdminPolicy, policy => policy.RequireRole(BookingRoles.Admin));
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("BookingService API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

/* WebApplicationFactory будет использоваться в интеграционных тестах */
public partial class Program;
