using BookingPlatform.BookingService.API.Contracts;
using BookingPlatform.BookingService.Bookings.Cancel;
using BookingPlatform.BookingService.Bookings.Complete;
using BookingPlatform.BookingService.Bookings.Confirm;
using BookingPlatform.BookingService.Bookings.Create;
using BookingPlatform.BookingService.Bookings.GetAll;
using BookingPlatform.BookingService.Bookings.GetById;
using BookingPlatform.BookingService.Bookings.GetHistory;
using BookingPlatform.BookingService.Bookings.GetMy;
using BookingPlatform.BookingService.Bookings.Reject;
using BookingPlatform.BookingService.Domain.Entities;
using BookingPlatform.BookingService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.BookingService.API.Controllers;

[ApiController]
[Route("")]
public sealed class BookingsController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly ISender _sender;

    public BookingsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var offer = new OfferSnapshot(
            request.OfferName,
            request.OfferDescription,
            request.DurationMinutes,
            request.Price,
            request.Currency,
            request.BufferBeforeMinutes,
            request.BufferAfterMinutes,
            request.CancellationDeadlineHours,
            request.RequiresConfirmation);

        var command = new CreateBookingCommand(
            request.ResourceId,
            request.OfferId,
            request.StartTime,
            offer,
            request.ClientName,
            request.ClientPhone,
            request.ClientEmail,
            request.Comment);

        var booking = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _sender.Send(new GetBookingByIdQuery(id), cancellationToken);

        if (booking is null)
            return NotFound();

        return Ok(booking);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy(
        BookingStatus? status, DateTimeOffset? from, DateTimeOffset? to,
        int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetMyBookingsQuery(status, from, to, NormalizePage(page), NormalizePageSize(pageSize));

        return Ok(await _sender.Send(query, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid? resourceId, BookingStatus? status, DateTimeOffset? from, DateTimeOffset? to,
        int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetAllBookingsQuery(resourceId, status, from, to, NormalizePage(page), NormalizePageSize(pageSize));

        return Ok(await _sender.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await _sender.Send(new GetBookingHistoryQuery(id), cancellationToken);

        if (history is null)
            return NotFound();

        return Ok(history);
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new ConfirmBookingCommand(id), cancellationToken));
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, ReasonRequest request, CancellationToken cancellationToken)
    {
        return NotFound();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, ReasonRequest request, CancellationToken cancellationToken)
    {
        return NotFound();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        return NotFound();
    }

    private static int NormalizePage(int page) => Math.Max(page, 1);

    private static int NormalizePageSize(int pageSize) => Math.Clamp(pageSize, 1, MaxPageSize);
}
