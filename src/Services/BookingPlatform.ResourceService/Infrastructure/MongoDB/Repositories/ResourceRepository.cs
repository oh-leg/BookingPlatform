using BookingPlatform.ResourceService.Application.Interfaces;
using BookingPlatform.ResourceService.Domain.Entities;
using MongoDB.Driver;

namespace BookingPlatform.ResourceService.Infrastructure.MongoDB.Repositories;

public sealed class ResourceRepository : IResourceRepository
{
    private readonly IMongoCollection<Resource> _resources;

    public ResourceRepository(MongoDbContext context)
    {
        _resources = context.GetCollection<Resource>("resources");
    }

    public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _resources
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Resource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _resources
            .Find(FilterDefinition<Resource>.Empty)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Resource resource, CancellationToken cancellationToken = default)
    {
        if (resource is null)
            throw new ArgumentNullException(nameof(resource));

        await _resources.InsertOneAsync(
            resource,
            cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Resource resource, CancellationToken cancellationToken = default)
    {
        if (resource is null)
            throw new ArgumentNullException(nameof(resource));

        await _resources.ReplaceOneAsync(
            x => x.Id == resource.Id,
            resource,
            cancellationToken: cancellationToken);
    }
}
