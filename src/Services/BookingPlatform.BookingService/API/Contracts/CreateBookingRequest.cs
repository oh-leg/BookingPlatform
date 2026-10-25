namespace BookingPlatform.BookingService.API.Contracts;

/*Поля offer* временные: пока нет связи с ResourceService, условия услуги передаёт клиент*/
public sealed record CreateBookingRequest(
    Guid ResourceId,
    Guid OfferId,
    DateTimeOffset StartTime,
    string OfferName,
    string? OfferDescription,
    int DurationMinutes,
    decimal Price,
    string Currency,
    int BufferBeforeMinutes,
    int BufferAfterMinutes,
    int CancellationDeadlineHours,
    bool RequiresConfirmation,
    string? ClientName,
    string? ClientPhone,
    string? ClientEmail,
    string? Comment);
