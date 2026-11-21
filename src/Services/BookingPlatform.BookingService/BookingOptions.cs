namespace BookingPlatform.BookingService;

public sealed class BookingOptions
{
    public const string SectionName = "Booking";

    /* Часовой пояс, в котором заданы недельные расписания услуг (IANA или Windows Id) */
    public string TimeZoneId { get; init; } = "Europe/Moscow";

    /* Максимальная длина периода при запросе слотов, дней */
    public int MaxSlotsRangeDays { get; init; } = 31;

    /* Максимальная длина периода при запросе списка броней, дней */
    public int MaxBookingsRangeDays { get; init; } = 92;

    /* Шаг нарезки слотов в минутах (MVP - 15) */
    public int SlotStepMinutes { get; init; } = 15;

    public TimeZoneInfo GetTimeZone() => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
}
