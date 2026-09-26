namespace BookingPlatform.ResourceService.Infrastructure.MongoDB;

public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; init; } = null!;

    public string DatabaseName { get; init; } = null!;
}