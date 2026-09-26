using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.Update;

public sealed record UpdateResourceCommand(Guid ResourceId, string Name, string? Description, string? Location, int? Capacity, Dictionary<string, string>? Metadata) : IRequest<bool>;
