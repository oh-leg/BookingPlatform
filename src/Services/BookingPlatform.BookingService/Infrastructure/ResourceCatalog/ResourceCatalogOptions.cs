namespace BookingPlatform.BookingService.Infrastructure.ResourceCatalog;

public sealed class ResourceCatalogOptions
{
    public const string SectionName = "ResourceCatalog";

    /* Внутренний адрес ResourceService */
    public string BaseUrl { get; init; } = "http://localhost:5001";

    public int TimeoutSeconds { get; init; } = 10;
}
