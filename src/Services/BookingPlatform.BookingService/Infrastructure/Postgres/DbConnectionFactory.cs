using System.Data;
using Dapper;
using Npgsql;

namespace BookingPlatform.BookingService.Infrastructure.Postgres;

public sealed class DbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    static DbConnectionFactory()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
    }

    public DbConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }

    /*timestamptz читаем и пишем только как UTC*/
    private sealed class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.Value = value.UtcDateTime;
        }

        public override DateTimeOffset Parse(object value)
        {
            return value switch
            {
                DateTime dt => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)),
                DateTimeOffset dto => dto.ToUniversalTime(),
                _ => throw new InvalidCastException($"Нельзя преобразовать {value.GetType()} в DateTimeOffset.")
            };
        }
    }
}
