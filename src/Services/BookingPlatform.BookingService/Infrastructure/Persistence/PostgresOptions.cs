namespace BookingPlatform.BookingService.Infrastructure.Persistence;

public sealed class PostgresOptions
{
    public const string SectionName = "Postgres";

    public const string ConnectionStringName = "BookingDb";

    /* SQL-миграция при старте сервиса применяется */
    public bool ApplyMigrationsOnStartup { get; init; } = true;
}
