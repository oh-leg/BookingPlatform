namespace BookingPlatform.ResourceService.API.Contracts.Resources;

public sealed record UpdateResourceRequest(string Name, string? Description, string? Location, int? Capacity, Dictionary<string, string>? Metadata);
