using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingPlatform.BookingService.Application.Interfaces;
using BookingPlatform.BookingService.Domain.Slots;
using BookingPlatform.BookingService.Infrastructure.ResourceCatalog.Dto;

namespace BookingPlatform.BookingService.Infrastructure.ResourceCatalog;

/* HTTP-клиент ResourceService. Обращается к сервису напрямую по внутренней сети. */
public sealed class ResourceCatalogClient : IResourceCatalogClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<ResourceCatalogClient> _logger;

    public ResourceCatalogClient(HttpClient httpClient, ILogger<ResourceCatalogClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ResourceSnapshot?> GetResourceAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.GetAsync($"api/resources/{resourceId}", cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            _logger.LogError(ex, "ResourceService недоступен");
            throw new ResourceCatalogUnavailableException("Сервис ресурсов недоступен.", ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ResourceService вернул {StatusCode} для ресурса {ResourceId}", (int)response.StatusCode, resourceId);
                throw new ResourceCatalogUnavailableException($"Сервис ресурсов вернул ошибку {(int)response.StatusCode}.");
            }

            ResourceDto? dto;

            try
            {
                dto = await response.Content.ReadFromJsonAsync<ResourceDto>(JsonOptions, cancellationToken);
            }
            catch (JsonException ex)
            {
                throw new ResourceCatalogUnavailableException("Некорректный ответ сервиса ресурсов.", ex);
            }

            return dto is null ? null : Map(dto);
        }
    }

    internal static ResourceSnapshot Map(ResourceDto dto) => new(
        dto.Id,
        dto.IsActive,
        (dto.Offers ?? []).Select(o => new OfferSnapshot(
                o.Id,
                o.Name,
                o.DurationMinutes,
                o.Price,
                o.Currency,
                o.BufferBeforeMinutes,
                o.BufferAfterMinutes,
                o.CancellationDeadlineHours,
                o.RequiresConfirmation,
                o.IsActive,
                (o.Schedule ?? []).Select(s => new WeeklyScheduleSnapshot(s.DayOfWeek, s.StartTime, s.EndTime, s.IsActive)).ToList()))
            .ToList(),
        (dto.ScheduleOverrides ?? [])
            .Where(x => Enum.IsDefined(typeof(ScheduleOverrideKind), x.Type))
            .Select(x => new ScheduleOverrideSnapshot(x.StartAt, x.EndAt, (ScheduleOverrideKind)x.Type, x.IsActive))
            .ToList());
}
