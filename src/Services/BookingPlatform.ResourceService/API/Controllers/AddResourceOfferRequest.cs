namespace BookingPlatform.ResourceService.API.Contracts.Resources;

public sealed record AddResourceOfferRequest(string Name, string? Description, int DurationMinutes, decimal Price, string Currency, int BufferBeforeMinutes, int BufferAfterMinutes, int CancellationDeadlineHours, bool RequiresConfirmation);
