using BookingPlatform.BookingService.Domain.Entities;
using MediatR;

namespace BookingPlatform.BookingService.Bookings.Create;

/*Условия услуги приходят в запросе, пока нет связи с ResourceService (потом их заменит gRPC)*/
public sealed record CreateBookingCommand(
    Guid ResourceId,
    Guid OfferId,
    DateTimeOffset StartTime,
    OfferSnapshot Offer,
    string? ClientName,
    string? ClientPhone,
    string? ClientEmail,
    string? Comment) : IRequest<BookingResponse>;
