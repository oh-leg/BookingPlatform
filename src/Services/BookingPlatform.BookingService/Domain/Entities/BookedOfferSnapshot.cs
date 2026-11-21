namespace BookingPlatform.BookingService.Domain.Entities;

/* Снимок параметров услуги на момент бронирования.
   Хранится вместе с бронью, чтобы последующие изменения услуги в ResourceService
   не меняли условия уже созданной брони (цена, дедлайн отмены и т.д.). */
public sealed record BookedOfferSnapshot(
    Guid OfferId,
    string OfferName,
    int DurationMinutes,
    decimal Price,
    string Currency,
    int BufferBeforeMinutes,
    int BufferAfterMinutes,
    int CancellationDeadlineHours,
    bool RequiresConfirmation);
