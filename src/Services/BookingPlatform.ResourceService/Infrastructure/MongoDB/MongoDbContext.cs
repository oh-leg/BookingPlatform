using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace BookingPlatform.ResourceService.Infrastructure.MongoDB;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbOptions> options)
    {
        var settings = options.Value;

        var client = new MongoClient(settings.ConnectionString);

        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
    
    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        await _database.RunCommandAsync<object>(
            new BsonDocument("ping", 1),
            cancellationToken: cancellationToken);
    }
}