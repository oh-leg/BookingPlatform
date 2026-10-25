using BookingPlatform.BookingService.API.ExceptionHandling;
using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Application.Users;
using BookingPlatform.BookingService.Bookings.Create;
using BookingPlatform.BookingService.Infrastructure.Auth;
using BookingPlatform.BookingService.Infrastructure.Postgres;
using BookingPlatform.BookingService.Infrastructure.Postgres.Repositories;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BookingDb")
    ?? throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:BookingDb.");

//Миграции BookingDB применяются при старте
DatabaseMigrator.Migrate(connectionString);

//Регистрация PostgreSQL
builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton<DbConnectionFactory>();

//Регистрация MediatR
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblyContaining<CreateBookingCommand>());

//Регистрация репозиториев
builder.Services.AddSingleton<IBookingRepository, BookingRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

//Текущий пользователь: пока демо-пользователь без JWT (для боевого режима вернуть CurrentUser)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, DemoCurrentUser>();
builder.Services.AddScoped<CurrentUserResolver>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler();

app.MapControllers();

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

app.UseHttpsRedirection();

app.Run();
