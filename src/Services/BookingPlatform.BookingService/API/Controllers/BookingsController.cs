using BookingPlatform.BookingService.API.Auth;
using BookingPlatform.BookingService.API.Controllers.Requests;
using BookingPlatform.BookingService.Application.Bookings;
using BookingPlatform.BookingService.Application.Bookings.Cancel;
using BookingPlatform.BookingService.Application.Bookings.Confirm;
using BookingPlatform.BookingService.Application.Bookings.Create;
using BookingPlatform.BookingService.Application.Bookings.GetById;
using BookingPlatform.BookingService.Application.Bookings.GetByResource;
using BookingPlatform.BookingService.Application.Bookings.GetHistory;
using BookingPlatform.BookingService.Application.Bookings.GetMy;
using BookingPlatform.BookingService.Application.Bookings.Reject;
using BookingPlatform.BookingService.Application.Slots.GetAvailableSlots;
using BookingPlatform.BookingService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookingPlatform.BookingService.API.Controllers;

/* Контроллер отвечает за создание бронирований, просмотр свободных слотов, получение истории и изменение статусов бронирования */
[ApiController]
[Authorize] /*обязательно аутентифицированный пользователь*/
[Route("")]
public sealed class BookingsController : ControllerBase
{
    private readonly ISender _sender;

    public BookingsController(ISender sender)
    {
        _sender = sender;
    }

    /* Свободные слоты услуги ресурса за период (часовом поясе расписания) */
    [HttpGet("slots")]
    [ProducesResponseType<IReadOnlyList<SlotResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlots(
        [FromQuery] Guid resourceId,
        [FromQuery] Guid offerId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
    {
        var slots = await _sender.Send(new GetAvailableSlotsQuery(resourceId, offerId, from, to), cancellationToken);
        return Ok(slots);
    }
    /*Создание брони*/
    [HttpPost]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await _sender.Send(
            new CreateBookingCommand(request.ResourceId, request.OfferId, request.StartAt, request.Comment),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    /* Сведения о моих бронях*/
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BookingResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMy(
        [FromQuery] BookingStatus? status,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var bookings = await _sender.Send(new GetMyBookingsQuery(status, from, to), cancellationToken);
        return Ok(bookings);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _sender.Send(new GetBookingByIdQuery(id), cancellationToken);
        return Ok(booking);
    }
    /*Запрос истории брони*/
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType<IReadOnlyList<BookingHistoryItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await _sender.Send(new GetBookingHistoryQuery(id), cancellationToken);
        return Ok(history);
    }
    /*Запрос отмены брони*/
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CancelBookingRequest? request,
        CancellationToken cancellationToken)
    {
        var booking = await _sender.Send(new CancelBookingCommand(id, request?.Reason), cancellationToken);
        return Ok(booking);
    }
    /*Запрос подтверждения брони*/
    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = BookingRoles.AdminPolicy)]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _sender.Send(new ConfirmBookingCommand(id), cancellationToken);
        return Ok(booking);
    }
    /*Запрос отклонения брони*/
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = BookingRoles.AdminPolicy)]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RejectBookingRequest? request,
        CancellationToken cancellationToken)
    {
        var booking = await _sender.Send(new RejectBookingCommand(id, request?.Reason), cancellationToken);
        return Ok(booking);
    }

    /* Все брони ресурса за период. Доступно только для admin */
    [HttpGet("resources/{resourceId:guid}")]
    [Authorize(Policy = BookingRoles.AdminPolicy)]
    [ProducesResponseType<IReadOnlyList<BookingResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByResource(
        Guid resourceId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var bookings = await _sender.Send(new GetResourceBookingsQuery(resourceId, from, to), cancellationToken);
        return Ok(bookings);
    }
}
