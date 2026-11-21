using BookingPlatform.BookingService.Domain.Entities;

namespace BookingPlatform.BookingService.Domain.Slots;

/* Данные ресурса из ResourceService, необходимые для расчета слотов.
*/
public sealed record ResourceSnapshot(
    Guid Id,
    bool IsActive,
    IReadOnlyCollection<OfferSnapshot> Offers,
    IReadOnlyCollection<ScheduleOverrideSnapshot> ScheduleOverrides)
{
    public OfferSnapshot? FindOffer(Guid offerId) => Offers.FirstOrDefault(x => x.Id == offerId);
}

public sealed record OfferSnapshot(
    Guid Id,
    string Name,
    int DurationMinutes,
    decimal Price,
    string Currency,
    int BufferBeforeMinutes,
    int BufferAfterMinutes,
    int CancellationDeadlineHours,
    bool RequiresConfirmation,
    bool IsActive,
    IReadOnlyCollection<WeeklyScheduleSnapshot> Schedule)
{
    /*
     * Копирую условия услуги на момент брони(фиксация требований)
     * 1. клиент забронировал услугу за 1000.
     * 2 администратор меняет цену услуги на 1200 .
     * 3 клиенту оказывается усллуга по кторой произошла бронь
     */
    public BookedOfferSnapshot ToBookedOffer() => new(
        Id,
        Name,
        DurationMinutes,
        Price,
        Currency,
        BufferBeforeMinutes,
        BufferAfterMinutes,
        CancellationDeadlineHours,
        RequiresConfirmation);
}

public sealed record WeeklyScheduleSnapshot(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);

public enum ScheduleOverrideKind
{
    Unavailable = 1,
    Available = 2
}

public sealed record ScheduleOverrideSnapshot(
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    ScheduleOverrideKind Kind,
    bool IsActive);
