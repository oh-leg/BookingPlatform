using BookingPlatform.ResourceService.Application.Interfaces;
using BookingPlatform.ResourceService.Application.Resources.GetById;
using BookingPlatform.ResourceService.Domain.Entities;
using NSubstitute;

namespace BookingPlatform.ResourceService.Tests.Unit.Application;

public class GetResourceByIdQueryHandlerTests
{
    private readonly IResourceRepository _repository = Substitute.For<IResourceRepository>();
    private readonly GetResourceByIdQueryHandler _handler;

    public GetResourceByIdQueryHandlerTests()
    {
        _handler = new GetResourceByIdQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ResourceExists_ReturnsResponseWithCorrectId()
    {
        var id = Guid.NewGuid();
        var resource = new Resource(id, new ResourceType(Guid.NewGuid(), "Type"), "Книга 1");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(resource);

        var result = await _handler.Handle(new GetResourceByIdQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Книга 1", result.Name);
    }

    [Fact]
    public async Task Handle_ResourceExists_MapsOffersAndOverrides()
    {
        var id = Guid.NewGuid();
        var resource = new Resource(id, new ResourceType(Guid.NewGuid(), "Type"), "Книга");
        resource.AddOffer(new ResourceOffer(Guid.NewGuid(), "Покупка 1", 60, 700, "RUB"));
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(resource);

        var result = await _handler.Handle(new GetResourceByIdQuery(id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result.Offers);
        Assert.Equal("Покупка 1", result.Offers.First().Name);
    }

    [Fact]
    public async Task Handle_ResourceNotFound_ReturnsNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Resource?)null);

        var result = await _handler.Handle(new GetResourceByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
