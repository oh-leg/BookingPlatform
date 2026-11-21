namespace BookingPlatform.BookingService.Domain.Exceptions;

/* Слот занят или бронь изменена */
public sealed class BookingConflictException : Exception
{
    public BookingConflictException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
