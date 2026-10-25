using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Entities;
using Dapper;

namespace BookingPlatform.BookingService.Infrastructure.Postgres.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User> GetOrCreateAsync(User candidate, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.OpenAsync(cancellationToken);

        // DO UPDATE нужен, чтобы RETURNING вернул строку и при гонке двух первых запросов
        var command = new CommandDefinition("""
            INSERT INTO users (id, keycloak_id, email, full_name, phone, created_at, updated_at)
            VALUES (@Id, @KeycloakId, @Email, @FullName, @Phone, @CreatedAt, @UpdatedAt)
            ON CONFLICT (keycloak_id) DO UPDATE SET keycloak_id = EXCLUDED.keycloak_id
            RETURNING id, keycloak_id, email, full_name, phone, created_at, updated_at
            """, candidate, cancellationToken: cancellationToken);

        return await connection.QuerySingleAsync<User>(command);
    }
}
