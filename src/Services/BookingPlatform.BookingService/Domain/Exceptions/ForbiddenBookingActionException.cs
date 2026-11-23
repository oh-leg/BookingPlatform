namespace BookingPlatform.BookingService.Domain.Exceptions;

/* ОТсутствуют права у пользователя */
public sealed class ForbiddenBookingActionException : Exception
{
    public ForbiddenBookingActionException(string message) : base(message)
    {
    }
}
