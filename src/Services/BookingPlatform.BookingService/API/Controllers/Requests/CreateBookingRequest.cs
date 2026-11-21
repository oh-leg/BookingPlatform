namespace BookingPlatform.BookingService.API.Controllers.Requests;

public sealed record CreateBookingRequest(Guid ResourceId, Guid OfferId, DateTimeOffset StartAt, string? Comment);
