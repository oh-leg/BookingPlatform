using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

// Временно
var keycloakHost = builder.Configuration["KEYCLOAK_HOSTNAME"] ?? "localhost";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"http://{keycloakHost}:8080/realms/booking-platform";
        options.Audience = "booking-api";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = $"http://{keycloakHost}:8080/realms/booking-platform"
        };
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
