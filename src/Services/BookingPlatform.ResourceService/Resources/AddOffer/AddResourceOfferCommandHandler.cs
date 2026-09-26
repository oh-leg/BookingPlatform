using BookingPlatform.ResourceService.Application.Interfaces;
using BookingPlatform.ResourceService.Domain.Entities;
using MediatR;

namespace BookingPlatform.ResourceService.Application.Resources.AddOffer;

public sealed class AddResourceOfferCommandHandler : IRequestHandler<AddResourceOfferCommand, Guid?>
{
    private readonly IResourceRepository _resourceRepository;

    public AddResourceOfferCommandHandler(IResourceRepository resourceRepository)
    {
        _resourceRepository = resourceRepository;
    }

    public async Task<Guid?> Handle(AddResourceOfferCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken);

        if (resource is null)
            return null;

        var offer = new ResourceOffer(Guid.NewGuid(), request.Name, request.DurationMinutes, request.Price, request.Currency, request.BufferBeforeMinutes, request.BufferAfterMinutes, request.CancellationDeadlineHours, request.RequiresConfirmation, request.Description);

        resource.AddOffer(offer);

        await _resourceRepository.UpdateAsync(resource, cancellationToken);

        return offer.Id;
    }
}
