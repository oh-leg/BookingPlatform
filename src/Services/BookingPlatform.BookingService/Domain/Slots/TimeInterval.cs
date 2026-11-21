namespace BookingPlatform.BookingService.Domain.Slots;

/* Полуоткрытый интервал времени [Start, End) в UTC.*/
public readonly record struct TimeInterval
{
    public TimeInterval(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new ArgumentException("Конец интервала должен быть больше начала.", nameof(end));

        Start = start.ToUniversalTime();
        End = end.ToUniversalTime();
    }

    public DateTimeOffset Start { get; }

    public DateTimeOffset End { get; }

    public bool Overlaps(TimeInterval other) => Start < other.End && other.Start < End;

    public bool Contains(TimeInterval other) => Start <= other.Start && other.End <= End;

    /* Объединение пересекающихся и смежных интервалов */
    public static List<TimeInterval> Merge(IEnumerable<TimeInterval> intervals)
    {
        var result = new List<TimeInterval>();

        foreach (var interval in intervals.OrderBy(x => x.Start))
        {
            if (result.Count > 0 && interval.Start <= result[^1].End)
            {
                var last = result[^1];
                if (interval.End > last.End)
                    result[^1] = new TimeInterval(last.Start, interval.End);
            }
            else
            {
                result.Add(interval);
            }
        }

        return result;
    }

    /* Разница между наборами интервалов */
    public static List<TimeInterval> Subtract(IEnumerable<TimeInterval> source, IReadOnlyCollection<TimeInterval> toRemove)
    {
        var result = new List<TimeInterval>();

        foreach (var interval in source)
        {
            var pieces = new List<TimeInterval> { interval };

            foreach (var removed in toRemove)
            {
                var next = new List<TimeInterval>();

                foreach (var piece in pieces)
                {
                    if (!piece.Overlaps(removed))
                    {
                        next.Add(piece);
                        continue;
                    }

                    if (piece.Start < removed.Start)
                        next.Add(new TimeInterval(piece.Start, removed.Start));

                    if (removed.End < piece.End)
                        next.Add(new TimeInterval(removed.End, piece.End));
                }

                pieces = next;
            }

            result.AddRange(pieces);
        }

        return result.OrderBy(x => x.Start).ToList();
    }
}
