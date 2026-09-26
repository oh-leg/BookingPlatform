using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.Create;

public sealed record CreateResourceCommand(string ResourceTypeName, string? ResourceTypeDescription, string Name, string? Description, string? Location, int? Capacity, Dictionary<string, string>? Metadata) : IRequest<Guid>;