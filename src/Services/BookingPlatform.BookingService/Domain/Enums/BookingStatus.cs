namespace BookingPlatform.BookingService.Domain.Enums;

public enum BookingStatus
{
    Pending = 1,        // Ожидает подтверждения
    Confirmed = 2,      // Подтверждено
    Rejected = 3,       // Отклонено администратором
    Cancelled = 4,      // Отменено
    Completed = 5       // Услуга оказана
}
