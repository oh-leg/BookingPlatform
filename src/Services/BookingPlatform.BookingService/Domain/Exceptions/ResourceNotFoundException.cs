namespace BookingPlatform.BookingService.Domain.Exceptions;

/* Ресурс или услуга не найдены в ResourceService */
public sealed class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string message) : base(message)
    {
    }
}
