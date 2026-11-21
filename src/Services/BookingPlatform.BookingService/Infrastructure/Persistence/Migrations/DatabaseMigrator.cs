using System.Reflection;
using Dapper;
using Npgsql;

namespace BookingPlatform.BookingService.Infrastructure.Persistence.Migrations;

/* Применяем встроенные в сборку SQL-скрипты Scripts/*.sql
   в порядке имен и запоминает их в schema_migrations.
   Использую Advisory lock исключает одновременное применение миграций несколькими экземплярами. */
public sealed class DatabaseMigrator
{
    private const long AdvisoryLockKey = 0x426F6F6B696E67; // "Booking"
    private const string ScriptsPrefix = "BookingPlatform.BookingService.Infrastructure.Persistence.Migrations.Scripts.";

    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(NpgsqlDataSource dataSource, ILogger<DatabaseMigrator> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            "SELECT pg_advisory_lock(@key)", new { key = AdvisoryLockKey }, cancellationToken: cancellationToken));

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                CREATE TABLE IF NOT EXISTS schema_migrations
                (
                    version    text        NOT NULL PRIMARY KEY,
                    applied_at timestamptz NOT NULL DEFAULT now()
                )
                """,
                cancellationToken: cancellationToken));

            var applied = (await connection.QueryAsync<string>(new CommandDefinition(
                    "SELECT version FROM schema_migrations", cancellationToken: cancellationToken)))
                .ToHashSet(StringComparer.Ordinal);

            foreach (var (version, sql) in LoadScripts())
            {
                if (applied.Contains(version))
                    continue;

                _logger.LogInformation("Применение миграции {Version}", version);

                await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                await connection.ExecuteAsync(new CommandDefinition(sql, transaction: transaction, cancellationToken: cancellationToken));
                await connection.ExecuteAsync(new CommandDefinition(
                    "INSERT INTO schema_migrations (version) VALUES (@version)",
                    new { version },
                    transaction,
                    cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);
            }
        }
        finally
        {
            await connection.ExecuteAsync(
                "SELECT pg_advisory_unlock(@key)", new { key = AdvisoryLockKey });
        }
    }

    private static IEnumerable<(string Version, string Sql)> LoadScripts()
    {
        var assembly = typeof(DatabaseMigrator).Assembly;

        var names = assembly.GetManifestResourceNames()
            .Where(x => x.StartsWith(ScriptsPrefix, StringComparison.Ordinal) && x.EndsWith(".sql", StringComparison.Ordinal))
            .OrderBy(x => x, StringComparer.Ordinal);

        foreach (var name in names)
        {
            using var stream = assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Не удалось прочитать миграцию {name}.");
            using var reader = new StreamReader(stream);

            var version = name[ScriptsPrefix.Length..^".sql".Length];

            yield return (version, reader.ReadToEnd());
        }
    }
}
