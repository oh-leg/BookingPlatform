using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Slots;

namespace BookingPlatform.BookingService.Application.Slots;

/* Общая логика расчета слотов для запроса слотов и создания брони:
   загрузка занятых интервалов ресурса и вызов SlotCalculator. */
internal static class SlotAvailability
{
    public static async Task<IReadOnlyList<TimeInterval>> CalculateAsync(
        ResourceSnapshot resource,
        Guid offerId,
        DateOnly from,
        DateOnly to,
        IBookingRepository bookingRepository,
        BookingOptions options,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var timeZone = options.GetTimeZone();
        var window = SlotCalculator.GetUtcWindow(from, to, timeZone);

        // Расширяем окно на сутки, чтобы учесть буферы броней на границах периода
        var occupied = await bookingRepository.GetOccupiedIntervalsAsync(
            resource.Id,
            window.Start.AddDays(-1),
            window.End.AddDays(1),
            cancellationToken);

        return SlotCalculator.Calculate(
            resource,
            offerId,
            from,
            to,
            timeZone,
            occupied,
            now,
            options.SlotStepMinutes);
    }
}
