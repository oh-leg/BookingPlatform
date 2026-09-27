using BookingPlatform.ResourceService.Domain.Entities;

namespace BookingPlatform.ResourceService.Application.Interfaces;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Resource>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Resource resource, CancellationToken cancellationToken = default);

    Task UpdateAsync(Resource resource, CancellationToken cancellationToken = default);
}
