namespace BookingPlatform.BookingService.Domain.Slots;

/* Динамический расчет свободных слотов услуги.
   Слоты не хранятся в БД, а вычисляются из недельного расписания услуги,
   исключений расписания ресурса и уже занятых интервалов (активных броней). */
public static class SlotCalculator
{
    /// <param name="resource">Ресурс с услугами и исключениями расписания.</param>
    /// <param name="offerId">Услуга, для которой считаются слоты.</param>
    /// <param name="from">Первая дата (локальная, в часовом поясе <paramref name="timeZone"/>).</param>
    /// <param name="to">Последняя дата включительно.</param>
    /// <param name="timeZone">Часовой пояс, в котором задано недельное расписание.</param>
    /// <param name="occupied">Занятые интервалы ресурса (с учетом буферов), UTC.</param>
    /// <param name="now">Текущее время: слоты, начинающиеся не позже него, отбрасываются.</param>
    /// <param name="stepMinutes">Шаг нарезки слотов; по умолчанию равен длительности услуги.</param>
    public static IReadOnlyList<TimeInterval> Calculate(
        ResourceSnapshot resource,
        Guid offerId,
        DateOnly from,
        DateOnly to,
        TimeZoneInfo timeZone,
        IReadOnlyCollection<TimeInterval> occupied,
        DateTimeOffset now,
        int? stepMinutes = null)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(timeZone);
        ArgumentNullException.ThrowIfNull(occupied);

        if (to < from)
            throw new ArgumentException("Дата окончания периода должна быть не раньше даты начала.", nameof(to));

        if (stepMinutes is <= 0)
            throw new ArgumentException("Шаг слотов должен быть больше нуля.", nameof(stepMinutes));

        var offer = resource.FindOffer(offerId);

        if (!resource.IsActive || offer is null || !offer.IsActive || offer.DurationMinutes <= 0)
            return [];

        var window = GetUtcWindow(from, to, timeZone);

        var working = new List<TimeInterval>();

        // 1. Недельное расписание услуги
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            foreach (var schedule in offer.Schedule)
            {
                if (!schedule.IsActive || schedule.DayOfWeek != date.DayOfWeek || schedule.EndTime <= schedule.StartTime)
                    continue;

                var start = ToUtc(date.ToDateTime(schedule.StartTime), timeZone);
                var end = ToUtc(date.ToDateTime(schedule.EndTime), timeZone);

                if (end > start)
                    working.Add(new TimeInterval(start, end));
            }
        }

        var activeOverrides = resource.ScheduleOverrides
            .Where(x => x.IsActive && x.EndAt > x.StartAt)
            .ToList();

        // 2. Исключения "доступен" добавляют рабочее время (в пределах запрошенного периода)
        foreach (var available in activeOverrides.Where(x => x.Kind == ScheduleOverrideKind.Available))
        {
            var start = Max(available.StartAt.ToUniversalTime(), window.Start);
            var end = Min(available.EndAt.ToUniversalTime(), window.End);

            if (end > start)
                working.Add(new TimeInterval(start, end));
        }

        // 3. Исключения "недоступен" вырезают рабочее время
        var unavailable = activeOverrides
            .Where(x => x.Kind == ScheduleOverrideKind.Unavailable)
            .Select(x => new TimeInterval(x.StartAt, x.EndAt))
            .ToList();

        var freeIntervals = TimeInterval.Subtract(TimeInterval.Merge(working), unavailable);

        // 4. Нарезка на слоты и фильтрация по занятости и текущему времени
        var duration = TimeSpan.FromMinutes(offer.DurationMinutes);
        var step = TimeSpan.FromMinutes(stepMinutes ?? offer.DurationMinutes);
        var bufferBefore = TimeSpan.FromMinutes(offer.BufferBeforeMinutes);
        var bufferAfter = TimeSpan.FromMinutes(offer.BufferAfterMinutes);

        var slots = new List<TimeInterval>();

        foreach (var interval in freeIntervals)
        {
            for (var slotStart = interval.Start; slotStart + duration <= interval.End; slotStart += step)
            {
                if (slotStart <= now)
                    continue;

                var slot = new TimeInterval(slotStart, slotStart + duration);
                var occupiedBySlot = new TimeInterval(slot.Start - bufferBefore, slot.End + bufferAfter);

                if (occupied.Any(x => x.Overlaps(occupiedBySlot)))
                    continue;

                slots.Add(slot);
            }
        }

        return slots;
    }

    /* Период [from 00:00; to+1 00:00) локального часового пояса в UTC */
    public static TimeInterval GetUtcWindow(DateOnly from, DateOnly to, TimeZoneInfo timeZone) =>
        new(
            ToUtc(from.ToDateTime(TimeOnly.MinValue), timeZone),
            ToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue), timeZone));

    /* Локальная дата момента времени в заданном часовом поясе */
    public static DateOnly ToLocalDate(DateTimeOffset moment, TimeZoneInfo timeZone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(moment, timeZone).DateTime);

    /* Перевод локального времени в UTC с учетом перехода на летнее время:
       несуществующее время (пропущенный час) сдвигается вперед. */
    private static DateTimeOffset ToUtc(DateTime local, TimeZoneInfo timeZone)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);

        while (timeZone.IsInvalidTime(unspecified))
            unspecified = unspecified.AddMinutes(30);

        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone), TimeSpan.Zero);
    }

    private static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;

    private static DateTimeOffset Min(DateTimeOffset a, DateTimeOffset b) => a < b ? a : b;
}
