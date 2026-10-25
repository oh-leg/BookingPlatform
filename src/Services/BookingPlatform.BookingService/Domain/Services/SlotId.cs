using System.Security.Cryptography;
using System.Text;

namespace BookingPlatform.BookingService.Domain.Services;

/*Слоты в БД не хранятся: идентификатор слота это UUID v5 от resource_id + offer_id + start_time (UTC)*/
public static class SlotId
{
    private static readonly Guid Namespace = new("6f1d7c52-3b8a-4e0e-9a54-0c1b2f6d8e41");

    public static Guid Create(Guid resourceId, Guid offerId, DateTimeOffset startTime)
    {
        var name = $"{resourceId:D}|{offerId:D}|{startTime.UtcDateTime:yyyy-MM-ddTHH:mm:ssZ}";
        var nameBytes = Encoding.UTF8.GetBytes(name);

        var data = new byte[16 + nameBytes.Length];
        Namespace.TryWriteBytes(data, bigEndian: true, out _);
        nameBytes.CopyTo(data, 16);

        var hash = SHA1.HashData(data);

        var bytes = hash.AsSpan(0, 16).ToArray();
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        return new Guid(bytes, bigEndian: true);
    }
}
