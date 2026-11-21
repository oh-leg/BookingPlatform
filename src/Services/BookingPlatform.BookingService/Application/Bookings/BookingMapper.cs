using BookingPlatform.BookingService.Domain.Entities;

namespace BookingPlatform.BookingService.Application.Bookings;

public static class BookingMapper
{
    public static BookingResponse ToResponse(this Booking booking) => new(
        booking.Id,
        booking.UserId,
        booking.ResourceId,
        booking.Offer.OfferId,
        booking.Offer.OfferName,
        booking.StartAt,
        booking.EndAt,
        booking.Status,
        booking.Offer.Price,
        booking.Offer.Currency,
        booking.Offer.RequiresConfirmation,
        booking.CancellationDeadline,
        booking.Comment,
        booking.CreatedAt,
        booking.UpdatedAt,
        booking.CancelledAt,
        booking.CancelledBy,
        booking.CancellationReason);

    public static BookingHistoryItemResponse ToResponse(this BookingStatusChange change) => new(
        change.FromStatus,
        change.ToStatus,
        change.ChangedBy,
        change.ChangedAt,
        change.Reason);
}
