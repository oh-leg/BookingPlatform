using BookingPlatform.ResourceService.Application.Interfaces;
using BookingPlatform.ResourceService.Application.Resources.GetAll;
using BookingPlatform.ResourceService.Domain.Entities;
using NSubstitute;

namespace BookingPlatform.ResourceService.Tests.Unit.Application;

public class GetResourcesQueryHandlerTests
{
    private readonly IResourceRepository _repository = Substitute.For<IResourceRepository>();
    private readonly GetResourcesQueryHandler _handler;

    public GetResourcesQueryHandlerTests()
    {
        _handler = new GetResourcesQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ResourcesExist_ReturnsMappedCollection()
    {
        var resources = new List<Resource>
        {
            new(Guid.NewGuid(), new ResourceType(Guid.NewGuid(), "Type"), "Книга 1"),
            new(Guid.NewGuid(), new ResourceType(Guid.NewGuid(), "Type"), "Книга 2")
        }.AsReadOnly();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(resources);

        var result = await _handler.Handle(new GetResourcesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_NoResources_ReturnsEmptyCollection()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Resource>().AsReadOnly());

        var result = await _handler.Handle(new GetResourcesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}