namespace BookingPlatform.BookingService.Domain.Entities;

/*Условия услуги на момент бронирования (копия ResourceOffer из ResourceService)*/
public sealed record OfferSnapshot(
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Price,
    string Currency,
    int BufferBeforeMinutes,
    int BufferAfterMinutes,
    int CancellationDeadlineHours,
    bool RequiresConfirmation);
