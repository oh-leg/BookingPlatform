namespace BookingPlatform.BookingService.Domain.Exceptions;

public sealed class BookingNotFoundException : Exception
{
    public BookingNotFoundException(string message) : base(message)
    {
    }
}
