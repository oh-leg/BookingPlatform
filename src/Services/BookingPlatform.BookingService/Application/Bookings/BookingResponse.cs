using BookingPlatform.BookingService.Domain.Enums;

namespace BookingPlatform.BookingService.Application.Bookings;

public sealed record BookingResponse(
    Guid Id,
    string UserId,
    Guid ResourceId,
    Guid OfferId,
    string OfferName,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    BookingStatus Status,
    decimal Price,
    string Currency,
    bool RequiresConfirmation,
    DateTimeOffset CancellationDeadline,
    string? Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CancelledAt,
    string? CancelledBy,
    string? CancellationReason);

public sealed record BookingHistoryItemResponse(
    BookingStatus? FromStatus,
    BookingStatus ToStatus,
    string ChangedBy,
    DateTimeOffset ChangedAt,
    string? Reason);
