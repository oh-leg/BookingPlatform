using BookingPlatform.BookingService.Domain.Slots;

namespace BookingPlatform.BookingService.Application.Interfaces;

/* Доступ к данным ResourceService */
public interface IResourceCatalogClient
{
    /*
     null - если ресурс не найден.
     ResourceCatalogUnavailableException, если ResourceService недоступен. 
    */
    Task<ResourceSnapshot?> GetResourceAsync(Guid resourceId, CancellationToken cancellationToken = default);
}

public sealed class ResourceCatalogUnavailableException : Exception
{
    public ResourceCatalogUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
