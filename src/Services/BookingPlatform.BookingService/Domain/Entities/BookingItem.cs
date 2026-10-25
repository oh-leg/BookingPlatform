namespace BookingPlatform.BookingService.Domain.Entities;

/*Строка брони со снимком цены на момент бронирования*/
public sealed class BookingItem
{
    private BookingItem()
    {
        // Для Dapper
    }

    public Guid Id { get; private set; }

    public Guid BookingId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public int Quantity { get; private set; }

    public decimal Total { get; private set; }

    public BookingItem(Guid bookingId, string name, string? description, decimal price, int quantity = 1)
    {
        if (bookingId == Guid.Empty)
            throw new ArgumentException("Идентификатор брони не может быть пустым.", nameof(bookingId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Наименование позиции не может быть пустым.", nameof(name));

        if (price < 0)
            throw new ArgumentException("Цена не может быть отрицательной.", nameof(price));

        if (quantity <= 0)
            throw new ArgumentException("Количество должно быть больше нуля.", nameof(quantity));

        Id = Guid.NewGuid();
        BookingId = bookingId;
        Name = name.Trim();
        Description = description?.Trim();
        Price = price;
        Quantity = quantity;
        Total = price * quantity;
    }
}
