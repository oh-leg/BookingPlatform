using System.Reflection;
using DbUp;

namespace BookingPlatform.BookingService.Infrastructure.Postgres;

public static class DatabaseMigrator
{
    /*Создаёт БД при необходимости и применяет встроенные SQL-миграции*/
    public static void Migrate(string connectionString)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var result = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToConsole()
            .Build()
            .PerformUpgrade();

        if (!result.Successful)
            throw new InvalidOperationException("Не удалось применить миграции BookingDB.", result.Error);
    }
}
