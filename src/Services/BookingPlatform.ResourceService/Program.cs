using BookingPlatform.ResourceService.API.ExceptionHandling;
using BookingPlatform.ResourceService.Application.Interfaces;
using BookingPlatform.ResourceService.Application.Resources.Create;
using BookingPlatform.ResourceService.Infrastructure.MongoDB;
using BookingPlatform.ResourceService.Infrastructure.MongoDB.Mappings;
using BookingPlatform.ResourceService.Infrastructure.MongoDB.Repositories;
using BookingPlatform.ResourceService.Application.Resources.Create;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Регистрация MongoDB
builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection(MongoDbOptions.SectionName));

//Регистрацияя MediatoR
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblyContaining<CreateResourceCommand>());

//регистрируем правила сериализации полей Domain->MongoDB
ResourceMongoMapping.Register();

//клиент MongoDB рассчитан на долгоживущую работу
builder.Services.AddSingleton<MongoDbContext>();

//Регистрация Resource
builder.Services.AddSingleton<IResourceRepository, ResourceRepository>();

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

    //вместо swagger буду использовать scalar
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("ResourceService API")
            .WithTheme(ScalarTheme.Kepler)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.Http);
    });
}

app.UseHttpsRedirection();

app.Run();
