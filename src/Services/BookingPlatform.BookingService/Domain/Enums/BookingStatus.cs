namespace BookingPlatform.BookingService.Domain.Enums;

/*значения совпадают с booking_statuses.id*/
public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    Rejected = 3,
    Cancelled = 4,
    Completed = 5
}
