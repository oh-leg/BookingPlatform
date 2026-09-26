using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("ResourceService API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Тестовый эндпоинт для SPA: доступен только после аутентификации на YARP-шлюзе
// (шлюз срезает /api/resources и проксирует запрос сюда как /test-data).
app.MapGet("/test-data", () => Results.Ok(new[]
    {
        new TestResource(1, "Переговорная «Альфа»", "room", true),
        new TestResource(2, "Проектор EPSON EB-X41", "equipment", true),
        new TestResource(3, "Иван Петров — DevOps-инженер", "specialist", false),
        new TestResource(4, "Переговорная «Бета»", "room", true)
    }))
    .WithName("GetTestData");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record TestResource(int Id, string Name, string Type, bool Available);
