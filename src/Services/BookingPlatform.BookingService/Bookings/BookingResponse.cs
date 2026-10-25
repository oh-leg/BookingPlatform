using BookingPlatform.BookingService.Domain.Entities;

namespace BookingPlatform.BookingService.Bookings;

public sealed record BookingItemResponse(string Name, string? Description, decimal Price, int Quantity, decimal Total);

public sealed record BookingResponse(
    Guid Id,
    Guid UserId,
    Guid ResourceId,
    Guid OfferId,
    Guid SlotId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    decimal TotalPrice,
    string Currency,
    string ClientName,
    string? ClientPhone,
    string ClientEmail,
    string? Comment,
    DateTimeOffset CancellationDeadlineAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<BookingItemResponse> Items)
{
    public static BookingResponse From(Booking b) => new(
        b.Id, b.UserId, b.ResourceId, b.OfferId, b.SlotId, b.StartTime, b.EndTime,
        b.Status.ToString(), b.TotalPrice, b.Currency, b.ClientName, b.ClientPhone, b.ClientEmail,
        b.Comment, b.CancellationDeadlineAt, b.CreatedAt, b.UpdatedAt,
        b.Items.Select(i => new BookingItemResponse(i.Name, i.Description, i.Price, i.Quantity, i.Total)).ToList());
}

public sealed record BookingHistoryResponse(
    string? OldStatus,
    string NewStatus,
    string? Reason,
    DateTimeOffset ChangedAt,
    Guid? ChangedBy);
