using BookingPlatform.ResourceService.API.Contracts.Resources;
using BookingPlatform.ResourceService.Application.Resources.Activate;
using BookingPlatform.ResourceService.Application.Resources.AddOffer;
using BookingPlatform.ResourceService.Application.Resources.AddSchedule;
using BookingPlatform.ResourceService.Application.Resources.AddScheduleOverride;
using BookingPlatform.ResourceService.Application.Resources.Create;
using BookingPlatform.ResourceService.Application.Resources.Deactivate;
using BookingPlatform.ResourceService.Application.Resources.GetAll;
using BookingPlatform.ResourceService.Application.Resources.GetById;
using BookingPlatform.ResourceService.Application.Resources.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.ResourceService.API.Controllers;

[ApiController]
[Route("api/resources")]
public sealed class ResourcesController : ControllerBase
{
    private readonly ISender _sender;

    public ResourcesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateResourceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateResourceCommand(
            request.ResourceTypeName,
            request.ResourceTypeDescription,
            request.Name,
            request.Description,
            request.Location,
            request.Capacity,
            request.Metadata);

        var id = await _sender.Send(command, cancellationToken);

        return Ok(id);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetResourcesQuery();
        var resources = await _sender.Send(query, cancellationToken);
        return Ok(resources);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetResourceByIdQuery(id);

        var resource = await _sender.Send(query, cancellationToken);

        if (resource is null)
            return NotFound();

        return Ok(resource);
    }

    [HttpPost("{resourceId:guid}/offers")]
    public async Task<IActionResult> AddOffer(Guid resourceId, AddResourceOfferRequest request, CancellationToken cancellationToken)
    {
        var command = new AddResourceOfferCommand(resourceId, request.Name, request.Description, request.DurationMinutes, request.Price, request.Currency, request.BufferBeforeMinutes, request.BufferAfterMinutes, request.CancellationDeadlineHours, request.RequiresConfirmation);

        var offerId = await _sender.Send(command, cancellationToken);

        if (offerId is null)
            return NotFound();

        return Ok(offerId);
    }

    [HttpPost("{resourceId:guid}/offers/{offerId:guid}/schedule")]
    public async Task<IActionResult> AddSchedule(Guid resourceId, Guid offerId, AddResourceScheduleRequest request, CancellationToken cancellationToken)
    {
        var command = new AddResourceScheduleCommand(resourceId, offerId, request.DayOfWeek, request.StartTime, request.EndTime);

        var scheduleId = await _sender.Send(command, cancellationToken);

        if (scheduleId is null)
            return NotFound();

        return Ok(scheduleId);
    }

    [HttpPost("{resourceId:guid}/schedule-overrides")]
    public async Task<IActionResult> AddScheduleOverride(Guid resourceId, AddScheduleOverrideRequest request, CancellationToken cancellationToken)
    {
        var command = new AddScheduleOverrideCommand(resourceId, request.StartAt, request.EndAt, request.Type, request.Reason);

        var overrideId = await _sender.Send(command, cancellationToken);

        if (overrideId is null)
            return NotFound();

        return Ok(overrideId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateResourceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateResourceCommand(id, request.Name, request.Description, request.Location, request.Capacity, request.Metadata);

        var found = await _sender.Send(command, cancellationToken);

        if (!found)
            return NotFound();

        return Ok();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var found = await _sender.Send(new ActivateResourceCommand(id), cancellationToken);

        if (!found)
            return NotFound();

        return Ok();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var found = await _sender.Send(new DeactivateResourceCommand(id), cancellationToken);

        if (!found)
            return NotFound();

        return Ok();
    }
}