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
        try
        {
            return await _resources
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }
        //System.TimeoutException бросает TimeOutException. Считаю что MongoDB не доступно
        catch (Exception ex) when (ex is MongoException or TimeoutException) { throw new DatabaseUnavailableException(ex); }
    }

    public async Task<IReadOnlyCollection<Resource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _resources
                .Find(FilterDefinition<Resource>.Empty)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException) { throw new DatabaseUnavailableException(ex); }
    }

    public async Task AddAsync(Resource resource, CancellationToken cancellationToken = default)
    {
        if (resource is null)
            throw new ArgumentNullException(nameof(resource));

        try
        {
            await _resources.InsertOneAsync(
                resource,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException) { throw new DatabaseUnavailableException(ex); }
    }

    public async Task UpdateAsync(Resource resource, CancellationToken cancellationToken = default)
    {
        if (resource is null)
            throw new ArgumentNullException(nameof(resource));

        try
        {
            await _resources.ReplaceOneAsync(
                x => x.Id == resource.Id,
                resource,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException) { throw new DatabaseUnavailableException(ex); }
    }
}
