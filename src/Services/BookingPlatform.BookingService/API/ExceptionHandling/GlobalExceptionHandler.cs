using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.BookingService.API.ExceptionHandling;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            BookingNotFoundException or ResourceNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ForbiddenBookingActionException => (StatusCodes.Status403Forbidden, "Forbidden"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            BookingConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            ResourceCatalogUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Service Unavailable"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Ошибка обработки запроса");

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            // Детали внутренних ошибок не показываю клиенту и сохраняем в систему логгирования
            Detail = statusCode == StatusCodes.Status500InternalServerError ? "Внутренняя ошибка сервера." : exception.Message
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
