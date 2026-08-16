using BookingPlatform.ResourceService.Domain.Entities;

namespace BookingPlatform.Tests.Unit.Domain.ResourceService;

public class ResourceOfferTests
{
    private static ResourceOffer CreateValidOffer()
    {
        return new ResourceOffer(
            id: Guid.NewGuid(),
            name: "Консультация",
            durationMinutes: 60,
            price: 2500,
            currency: "RUB");
    }

    private static ResourceSchedule CreateSchedule(
        DayOfWeek dayOfWeek = DayOfWeek.Monday,
        int startHour = 9,
        int endHour = 18)
    {
        return new ResourceSchedule(
            Guid.NewGuid(),
            dayOfWeek,
            new TimeOnly(startHour, 0),
            new TimeOnly(endHour, 0));
    }
    
// Constructor

    [Fact]
    public void Constructor_ValidData_CreatesOffer()
    {
        var id = Guid.NewGuid();
        
        var offer = new ResourceOffer(
            id,
            "  Консультация  ",
            60,
            2500,
            " rub ");
        
        Assert.Equal(id, offer.Id);
        Assert.Equal("Консультация", offer.Name);
        Assert.Equal(60, offer.DurationMinutes);
        Assert.Equal(2500, offer.Price);
        Assert.Equal("RUB", offer.Currency);

        Assert.Equal(0, offer.BufferBeforeMinutes);
        Assert.Equal(0, offer.BufferAfterMinutes);
        Assert.Equal(0, offer.CancellationDeadlineHours);

        Assert.False(offer.RequiresConfirmation);
        Assert.True(offer.IsActive);
        Assert.Empty(offer.Schedule);

        Assert.True(offer.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.Empty,
            "Консультация",
            60,
            2500,
            "RUB");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.NewGuid(),
            "",
            60,
            2500,
            "RUB");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhitespaceName_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.NewGuid(),
            "   ",
            60,
            2500,
            "RUB");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_ZeroDuration_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            0,
            2500,
            "RUB");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("durationMinutes", exception.ParamName);
    }

    [Fact]
    public void Constructor_NegativeDuration_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            -1,
            2500,
            "RUB");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("durationMinutes", exception.ParamName);
    }

    [Fact]
    public void Constructor_ZeroPrice_CreatesOffer()
    {
        var offer = new ResourceOffer(
            Guid.NewGuid(),
            "Бесплатная консультация",
            60,
            0,
            "RUB");
        
        Assert.Equal(0, offer.Price);
    }

    [Fact]
    public void Constructor_NegativePrice_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            -1,
            "RUB");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void Constructor_EmptyCurrency_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("currency", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhitespaceCurrency_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "   ");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("currency", exception.ParamName);
    }

    [Fact]
    public void Constructor_CurrencyWithSpaces_NormalizesCurrency()
    {
        var offer = new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            " rub ");
        
        Assert.Equal("RUB", offer.Currency);
    }

    [Fact]
    public void Constructor_NegativeBufferBefore_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "RUB",
            bufferBeforeMinutes: -1);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("bufferBeforeMinutes", exception.ParamName);
    }

    [Fact]
    public void Constructor_NegativeBufferAfter_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "RUB",
            bufferAfterMinutes: -1);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("bufferAfterMinutes", exception.ParamName);
    }

    [Fact]
    public void Constructor_NegativeCancellationDeadline_ThrowsArgumentException()
    {
        var action = () => new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "RUB",
            cancellationDeadlineHours: -1);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal(
            "cancellationDeadlineHours",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_ValidOptionalParameters_SetsValues()
    {
        var offer = new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            90,
            3500,
            "EUR",
            bufferBeforeMinutes: 15,
            bufferAfterMinutes: 30,
            cancellationDeadlineHours: 24,
            requiresConfirmation: true,
            description: "  Первичная консультация  ");
        
        Assert.Equal(90, offer.DurationMinutes);
        Assert.Equal(3500, offer.Price);
        Assert.Equal("EUR", offer.Currency);

        Assert.Equal(15, offer.BufferBeforeMinutes);
        Assert.Equal(30, offer.BufferAfterMinutes);
        Assert.Equal(24, offer.CancellationDeadlineHours);

        Assert.True(offer.RequiresConfirmation);
        Assert.Equal(
            "Первичная консультация",
            offer.Description);
    }
    
    // Rename

    [Fact]
    public void Rename_ValidName_ChangesName()
    {
        var offer = CreateValidOffer();
        
        offer.Rename("  Новое название  ");
        
        Assert.Equal("Новое название", offer.Name);
    }

    [Fact]
    public void Rename_EmptyName_ThrowsArgumentException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.Rename("");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Rename_WhitespaceName_ThrowsArgumentException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.Rename("   ");
        
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Rename_InvalidName_DoesNotChangeExistingName()
    {
        var offer = CreateValidOffer();
        var originalName = offer.Name;
        
        var action = () => offer.Rename("   ");
        
        Assert.Throws<ArgumentException>(action);
        Assert.Equal(originalName, offer.Name);
    }
    
    // Description
    
    [Fact]
    public void UpdateDescription_ValidDescription_UpdatesDescription()
    {
        var offer = CreateValidOffer();
        
        offer.UpdateDescription("  Новое описание  ");
        
        Assert.Equal("Новое описание", offer.Description);
    }

    [Fact]
    public void UpdateDescription_Null_ClearsDescription()
    {
        var offer = new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "RUB",
            description: "Описание");
        
        offer.UpdateDescription(null);
        
        Assert.Null(offer.Description);
    }
    
    // Duration

    [Fact]
    public void ChangeDuration_ValidValue_ChangesDuration()
    {
        var offer = CreateValidOffer();
        
        offer.ChangeDuration(90);
        
        Assert.Equal(90, offer.DurationMinutes);
    }

    [Fact]
    public void ChangeDuration_Zero_ThrowsArgumentException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.ChangeDuration(0);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("durationMinutes", exception.ParamName);
    }

    [Fact]
    public void ChangeDuration_NegativeValue_ThrowsArgumentException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.ChangeDuration(-10);
        
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ChangeDuration_InvalidValue_DoesNotChangeExistingDuration()
    {
        var offer = CreateValidOffer();
        var originalDuration = offer.DurationMinutes;
        
        var action = () => offer.ChangeDuration(0);
        
        Assert.Throws<ArgumentException>(action);
        Assert.Equal(originalDuration, offer.DurationMinutes);
    }
    
    // Price

    [Fact]
    public void ChangePrice_ValidValue_ChangesPrice()
    {
        var offer = CreateValidOffer();
        
        offer.ChangePrice(3500);
        
        Assert.Equal(3500, offer.Price);
    }

    [Fact]
    public void ChangePrice_Zero_SetsZeroPrice()
    {
        var offer = CreateValidOffer();
        
        offer.ChangePrice(0);
        
        Assert.Equal(0, offer.Price);
    }

    [Fact]
    public void ChangePrice_NegativeValue_ThrowsArgumentException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.ChangePrice(-1);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void ChangePrice_InvalidValue_DoesNotChangeExistingPrice()
    {
        var offer = CreateValidOffer();
        var originalPrice = offer.Price;
        
        var action = () => offer.ChangePrice(-100);
        
        Assert.Throws<ArgumentException>(action);
        Assert.Equal(originalPrice, offer.Price);
    }
    
    // Currency

    [Fact]
    public void ChangeCurrency_ValidValue_ChangesCurrency()
    {
        var offer = CreateValidOffer();
        
        offer.ChangeCurrency("eur");
        
        Assert.Equal("EUR", offer.Currency);
    }

    [Fact]
    public void ChangeCurrency_TrimsAndUpperCasesCurrency()
    {
        var offer = CreateValidOffer();
        
        offer.ChangeCurrency("  usd  ");
        
        Assert.Equal("USD", offer.Currency);
    }

    [Fact]
    public void ChangeCurrency_EmptyCurrency_ThrowsArgumentException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.ChangeCurrency("");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("currency", exception.ParamName);
    }

    [Fact]
    public void ChangeCurrency_WhitespaceCurrency_ThrowsArgumentException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.ChangeCurrency("   ");
        
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ChangeCurrency_InvalidValue_DoesNotChangeExistingCurrency()
    {
        var offer = CreateValidOffer();
        var originalCurrency = offer.Currency;
        
        var action = () => offer.ChangeCurrency("   ");
        
        Assert.Throws<ArgumentException>(action);
        Assert.Equal(originalCurrency, offer.Currency);
    }
    
    // Buffers

    [Fact]
    public void ChangeBuffers_ValidValues_ChangesBuffers()
    {
        var offer = CreateValidOffer();
        
        offer.ChangeBuffers(
            bufferBeforeMinutes: 15,
            bufferAfterMinutes: 30);
        
        Assert.Equal(15, offer.BufferBeforeMinutes);
        Assert.Equal(30, offer.BufferAfterMinutes);
    }

    [Fact]
    public void ChangeBuffers_ZeroValues_AcceptsValues()
    {
        var offer = new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "RUB",
            bufferBeforeMinutes: 15,
            bufferAfterMinutes: 30);
        
        offer.ChangeBuffers(0, 0);
        
        Assert.Equal(0, offer.BufferBeforeMinutes);
        Assert.Equal(0, offer.BufferAfterMinutes);
    }

    [Fact]
    public void ChangeBuffers_NegativeBufferBefore_ThrowsArgumentException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.ChangeBuffers(-1, 30);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("bufferBeforeMinutes", exception.ParamName);
    }

    [Fact]
    public void ChangeBuffers_NegativeBufferAfter_ThrowsArgumentException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.ChangeBuffers(15, -1);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("bufferAfterMinutes", exception.ParamName);
    }

    [Fact]
    public void ChangeBuffers_InvalidBufferBefore_DoesNotChangeExistingBuffers()
    {
        var offer = new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "RUB",
            bufferBeforeMinutes: 10,
            bufferAfterMinutes: 20);
        
        var action = () => offer.ChangeBuffers(-1, 30);
        
        Assert.Throws<ArgumentException>(action);

        Assert.Equal(10, offer.BufferBeforeMinutes);
        Assert.Equal(20, offer.BufferAfterMinutes);
    }

    [Fact]
    public void ChangeBuffers_InvalidBufferAfter_DoesNotChangeExistingBuffers()
    {
        var offer = new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "RUB",
            bufferBeforeMinutes: 10,
            bufferAfterMinutes: 20);
        
        var action = () => offer.ChangeBuffers(30, -1);
        
        Assert.Throws<ArgumentException>(action);

        Assert.Equal(10, offer.BufferBeforeMinutes);
        Assert.Equal(20, offer.BufferAfterMinutes);
    }
    
    // Cancellation deadline

    [Fact]
    public void ChangeCancellationDeadline_ValidValue_ChangesDeadline()
    {
        var offer = CreateValidOffer();
        
        offer.ChangeCancellationDeadline(24);
        
        Assert.Equal(24, offer.CancellationDeadlineHours);
    }

    [Fact]
    public void ChangeCancellationDeadline_Zero_SetsZero()
    {
        var offer = new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "RUB",
            cancellationDeadlineHours: 24);
        
        offer.ChangeCancellationDeadline(0);
        
        Assert.Equal(0, offer.CancellationDeadlineHours);
    }

    [Fact]
    public void ChangeCancellationDeadline_NegativeValue_ThrowsArgumentException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.ChangeCancellationDeadline(-1);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("hours", exception.ParamName);
    }

    [Fact]
    public void ChangeCancellationDeadline_InvalidValue_DoesNotChangeExistingDeadline()
    {
        var offer = new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "RUB",
            cancellationDeadlineHours: 24);
        
        var action = () => offer.ChangeCancellationDeadline(-1);
        
        Assert.Throws<ArgumentException>(action);

        Assert.Equal(24, offer.CancellationDeadlineHours);
    }
    
    // Confirmation

    [Fact]
    public void SetConfirmationRequired_True_SetsRequirement()
    {
        var offer = CreateValidOffer();
        
        offer.SetConfirmationRequired(true);
        
        Assert.True(offer.RequiresConfirmation);
    }

    [Fact]
    public void SetConfirmationRequired_False_RemovesRequirement()
    {
        var offer = new ResourceOffer(
            Guid.NewGuid(),
            "Консультация",
            60,
            2500,
            "RUB",
            requiresConfirmation: true);
        
        offer.SetConfirmationRequired(false);
        
        Assert.False(offer.RequiresConfirmation);
    }

    // Deactivate

    [Fact]
    public void Deactivate_ActiveOffer_DeactivatesOffer()
    {
        var offer = CreateValidOffer();
        offer.Deactivate();
        
        Assert.False(offer.IsActive);
    }

    [Fact]
    public void Deactivate_AlreadyInactiveOffer_RemainsInactive()
    {
        var offer = CreateValidOffer();

        offer.Deactivate();
        offer.Deactivate();
        
        Assert.False(offer.IsActive);
    }
    
    // InActivate
    [Fact]
    public void Activate_InactiveOffer_ActivatesOffer()
    {
        var offer = CreateValidOffer();

        offer.Deactivate();
        offer.Activate();
        
        Assert.True(offer.IsActive);
    }

    [Fact]
    public void Activate_AlreadyActiveOffer_RemainsActive()
    {
        var offer = CreateValidOffer();
        
        offer.Activate();
        
        Assert.True(offer.IsActive);
    }
    
    // AddSchedule

    [Fact]
    public void AddSchedule_ValidSchedule_AddsSchedule()
    {
        var offer = CreateValidOffer();

        var schedule = CreateSchedule();
        
        offer.AddSchedule(schedule);
        
        Assert.Single(offer.Schedule);
        Assert.Contains(schedule, offer.Schedule);
    }

    [Fact]
    public void AddSchedule_NullSchedule_ThrowsArgumentNullException()
    {
        var offer = CreateValidOffer();
        
        var action = () => offer.AddSchedule(null!);
        
        var exception = Assert.Throws<ArgumentNullException>(action);

        Assert.Equal("schedule", exception.ParamName);
    }

    [Fact]
    public void AddSchedule_OverlappingAtEnd_ThrowsInvalidOperationException()
    {
        var offer = CreateValidOffer();

        var existing = CreateSchedule(DayOfWeek.Monday, 9, 12);

        var overlapping = CreateSchedule(DayOfWeek.Monday, 10, 13);

        offer.AddSchedule(existing);
        
        var action = () => offer.AddSchedule(overlapping);
        
        var exception = Assert.Throws<InvalidOperationException>(action);

        Assert.Equal("Указанный интервал расписания пересекается с уже существующим.", exception.Message);

        Assert.Single(offer.Schedule);
    }

    [Fact]
    public void AddSchedule_OverlappingAtStart_ThrowsInvalidOperationException()
    {
        var offer = CreateValidOffer();

        var existing = CreateSchedule(DayOfWeek.Monday, 9, 12);

        var overlapping = CreateSchedule(DayOfWeek.Monday, 7, 10);

        offer.AddSchedule(existing);
        
        var action = () => offer.AddSchedule(overlapping);
        
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddSchedule_NewIntervalInsideExisting_ThrowsInvalidOperationException()
    {
        var offer = CreateValidOffer();

        var existing = CreateSchedule(DayOfWeek.Monday, 9, 12);

        var overlapping = CreateSchedule(DayOfWeek.Monday, 10, 11);

        offer.AddSchedule(existing);
        
        var action = () => offer.AddSchedule(overlapping);
        
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddSchedule_NewIntervalContainsExisting_ThrowsInvalidOperationException()
    {
        var offer = CreateValidOffer();

        var existing = CreateSchedule(DayOfWeek.Monday, 9, 12);

        var overlapping = CreateSchedule(DayOfWeek.Monday, 8, 13);

        offer.AddSchedule(existing);
        
        var action = () => offer.AddSchedule(overlapping);
        
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddSchedule_SameInterval_ThrowsInvalidOperationException()
    {
        var offer = CreateValidOffer();

        var existing = CreateSchedule(DayOfWeek.Monday, 9, 12);

        var duplicate = CreateSchedule(DayOfWeek.Monday, 9, 12);

        offer.AddSchedule(existing);
        
        var action = () => offer.AddSchedule(duplicate);
        
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddSchedule_AdjacentIntervals_AddsSchedule()
    {
        var offer = CreateValidOffer();

        var first = CreateSchedule(DayOfWeek.Monday, 9, 12);

        var second = CreateSchedule(DayOfWeek.Monday, 12, 15);

        offer.AddSchedule(first);
        offer.AddSchedule(second);
        
        Assert.Equal(2, offer.Schedule.Count);
        Assert.Contains(first, offer.Schedule);
        Assert.Contains(second, offer.Schedule);
    }

    [Fact]
    public void AddSchedule_SameTimeDifferentDays_AddsSchedule()
    {
        var offer = CreateValidOffer();

        var monday = CreateSchedule(DayOfWeek.Monday, 9, 18);

        var tuesday = CreateSchedule(DayOfWeek.Tuesday, 9, 18);
        
        offer.AddSchedule(monday);
        offer.AddSchedule(tuesday);
        
        Assert.Equal(2, offer.Schedule.Count);
    }

    [Fact]
    public void AddSchedule_OverlapsInactiveSchedule_AddsSchedule()
    {
        var offer = CreateValidOffer();

        var inactiveSchedule = CreateSchedule(DayOfWeek.Monday, 9, 12);

        inactiveSchedule.Deactivate();

        var newSchedule = CreateSchedule(DayOfWeek.Monday, 10, 13);

        offer.AddSchedule(inactiveSchedule);
        offer.AddSchedule(newSchedule);
        
        Assert.Equal(2, offer.Schedule.Count);
        Assert.Contains(inactiveSchedule, offer.Schedule);
        Assert.Contains(newSchedule, offer.Schedule);
    }
}