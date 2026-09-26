namespace BookingPlatform.ResourceService.API.Contracts.Resources;

public sealed record CreateResourceRequest(string ResourceTypeName, string? ResourceTypeDescription, string Name, string? Description, string? Location, int? Capacity, Dictionary<string, string>? Metadata);