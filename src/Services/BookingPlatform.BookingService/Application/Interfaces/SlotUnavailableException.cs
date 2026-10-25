namespace BookingPlatform.BookingService.Application.Interfaces;

public sealed class SlotUnavailableException() : InvalidOperationException("Выбранное время уже занято.");
