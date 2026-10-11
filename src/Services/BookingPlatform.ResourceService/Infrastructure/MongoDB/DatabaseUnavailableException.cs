namespace BookingPlatform.ResourceService.Infrastructure.MongoDB;

public sealed class DatabaseUnavailableException(Exception innerException)
    : Exception("На текущий момент база данных не доступна. Попробуйте позже.", innerException);
