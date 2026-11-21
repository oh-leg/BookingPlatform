using Microsoft.Extensions.Options;
using Npgsql;

namespace BookingPlatform.BookingService.Infrastructure.Persistence.Migrations;

/* Применяем скрипт миграции при старте сервиса (до начала приема запросов) */
public sealed class DatabaseMigrationHostedService : IHostedService
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    private readonly DatabaseMigrator _migrator;
    private readonly PostgresOptions _options;
    private readonly ILogger<DatabaseMigrationHostedService> _logger;

    public DatabaseMigrationHostedService(
        DatabaseMigrator migrator,
        IOptions<PostgresOptions> options,
        ILogger<DatabaseMigrationHostedService> logger)
    {
        _migrator = migrator;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.ApplyMigrationsOnStartup)
            return;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await _migrator.MigrateAsync(cancellationToken);
                return;
            }
            catch (NpgsqlException ex) when (attempt < MaxAttempts && ex is not PostgresException)
            {
                // База может быть еще не готова (например, при старте docker compose)
                _logger.LogWarning(ex, "База данных недоступна, попытка {Attempt} из {MaxAttempts}", attempt, MaxAttempts);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
