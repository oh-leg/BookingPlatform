using BookingPlatform.ResourceService.Domain.Entities;
using BookingPlatform.ResourceService.Domain.Enums;

namespace BookingPlatform.Tests.Unit.Domain.ResourceService;

public class ResourceTests
{
    private static Resource CreateValidResource()
    {
        return new Resource(
            id: Guid.NewGuid(),
            resourceTypeId: Guid.NewGuid(),
            name: "Toyota Camry");
    }

    private static ResourceOffer CreateValidOffer(string name = "Почасовая аренда")
    {
        return new ResourceOffer(
            id: Guid.NewGuid(),
            name: name,
            durationMinutes: 60,
            price: 2500,
            currency: "RUB");
    }

    private static ResourceScheduleOverride CreateScheduleOverride(Guid resourceId, int startHour = 10, int endHour = 12)
    {
        return new ResourceScheduleOverride(
            id: Guid.NewGuid(),
            resourceId: resourceId,
            startAt: new DateTimeOffset(2026, 8, 16, startHour, 0, 0, TimeSpan.FromHours(3)),
            endAt: new DateTimeOffset(2026, 8, 16, endHour, 0, 0, TimeSpan.FromHours(3)),
            type: ScheduleOverrideType.Unavailable,
            reason: "Техническое обслуживание");
    }
    
    // Constructor

    [Fact]
    public void Constructor_ValidData_CreatesResource()
    {
        var id = Guid.NewGuid();
        var resourceTypeId = Guid.NewGuid();

        var metadata = new Dictionary<string, object>
        {
            ["brand"] = "Toyota",
            ["model"] = "Camry"
        };
        
        var resource = new Resource(
            id,
            resourceTypeId,
            "  Toyota Camry  ",
            description: "  Автомобиль бизнес-класса  ",
            location: "  Москва  ",
            capacity: 5,
            metadata: metadata);
        
        Assert.Equal(id, resource.Id);
        Assert.Equal(resourceTypeId, resource.ResourceTypeId);

        Assert.Equal("Toyota Camry", resource.Name);
        Assert.Equal("Автомобиль бизнес-класса", resource.Description);

        Assert.Equal("Москва", resource.Location);
        Assert.Equal(5, resource.Capacity);
        Assert.Same(metadata, resource.Metadata);

        Assert.True(resource.IsActive);
        Assert.Empty(resource.Offers);
        Assert.Empty(resource.ScheduleOverrides);

        Assert.True(resource.CreatedAt <= DateTimeOffset.UtcNow);
    }
    
    [Fact]
    public void Constructor_EmptyId_ThrowsArgumentException()
    {
        var action = () => new Resource(
            Guid.Empty,
            Guid.NewGuid(),
            "Toyota Camry");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("id", exception.ParamName);
    }


    [Fact]
    public void Constructor_EmptyResourceTypeId_ThrowsArgumentException()
    {
        var action = () => new Resource(
            Guid.NewGuid(),
            Guid.Empty,
            "Toyota Camry");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("resourceTypeId", exception.ParamName);
    }


    [Fact]
    public void Constructor_EmptyName_ThrowsArgumentException()
    {
        var action = () => new Resource(Guid.NewGuid(), Guid.NewGuid(), "");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("name", exception.ParamName);
    }


    [Fact]
    public void Constructor_WhitespaceName_ThrowsArgumentException()
    {
        var action = () => new Resource(Guid.NewGuid(), Guid.NewGuid(), "   ");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("name", exception.ParamName);
    }


    [Fact]
    public void Constructor_ZeroCapacity_ThrowsArgumentException()
    {
        var action = () => new Resource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Toyota Camry",
            capacity: 0);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("capacity", exception.ParamName);
    }


    [Fact]
    public void Constructor_NegativeCapacity_ThrowsArgumentException()
    {
        var action = () => new Resource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Toyota Camry",
            capacity: -1);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("capacity", exception.ParamName);
    }


    [Fact]
    public void Constructor_NullCapacity_CreatesResource()
    {
        var resource = new Resource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Toyota Camry",
            capacity: null);
        
        Assert.Null(resource.Capacity);
    }


    [Fact]
    public void Constructor_TrimsDescriptionAndLocation()
    {
        var resource = new Resource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Resource",
            description: "  Description  ",
            location: "  Moscow  ");
        
        Assert.Equal("Description", resource.Description);
        Assert.Equal("Moscow", resource.Location);
    }
    
    // Rename

    [Fact]
    public void Rename_ValidName_ChangesName()
    {
        var resource = CreateValidResource();
        
        resource.Rename("  New Resource Name  ");
        
        Assert.Equal("New Resource Name", resource.Name);
    }


    [Fact]
    public void Rename_EmptyName_ThrowsArgumentException()
    {
        var resource = CreateValidResource();
        
        var action = () => resource.Rename("");
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("name", exception.ParamName);
    }


    [Fact]
    public void Rename_InvalidName_DoesNotChangeExistingName()
    {
        var resource = CreateValidResource();
        var originalName = resource.Name;
        
        var action = () => resource.Rename("   ");
        
        Assert.Throws<ArgumentException>(action);
        Assert.Equal(originalName, resource.Name);
    }
    
    // Description

    [Fact]
    public void UpdateDescription_ValidDescription_UpdatesDescription()
    {
        var resource = CreateValidResource();
        
        resource.UpdateDescription("  New description  ");
        
        Assert.Equal("New description", resource.Description);
    }
    
    [Fact]
    public void UpdateDescription_Null_ClearsDescription()
    {
        var resource = new Resource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Resource",
            description: "Description");
        
        resource.UpdateDescription(null);
        
        Assert.Null(resource.Description);
    }
    
    // Location

    [Fact]
    public void UpdateLocation_ValidLocation_UpdatesLocation()
    {
        var resource = CreateValidResource();
        
        resource.UpdateLocation("  Moscow  ");
        
        Assert.Equal("Moscow", resource.Location);
    }

    [Fact]
    public void UpdateLocation_Null_ClearsLocation()
    {
        var resource = new Resource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Resource",
            location: "Moscow");
        
        resource.UpdateLocation(null);
        
        Assert.Null(resource.Location);
    }
    
    // Capacity

    [Fact]
    public void UpdateCapacity_ValidValue_ChangesCapacity()
    {
        var resource = new Resource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Resource",
            capacity: 5);
        
        resource.UpdateCapacity(10);
        
        Assert.Equal(10, resource.Capacity);
    }
    
    [Fact]
    public void UpdateCapacity_Null_ClearsCapacity()
    {
        var resource = new Resource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Resource",
            capacity: 5);
        
        resource.UpdateCapacity(null);
        
        Assert.Null(resource.Capacity);
    }
    
    [Fact]
    public void UpdateCapacity_Zero_ThrowsArgumentException()
    {
        var resource = CreateValidResource();
        
        var action = () => resource.UpdateCapacity(0);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("capacity", exception.ParamName);
    }
    
    [Fact]
    public void UpdateCapacity_NegativeValue_ThrowsArgumentException()
    {
        var resource = CreateValidResource();
        
        var action = () => resource.UpdateCapacity(-1);
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("capacity", exception.ParamName);
    }
    
    [Fact]
    public void UpdateCapacity_InvalidValue_DoesNotChangeExistingCapacity()
    {
        var resource = new Resource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Resource",
            capacity: 5);
        
        var action = () => resource.UpdateCapacity(0);
        
        Assert.Throws<ArgumentException>(action);
        Assert.Equal(5, resource.Capacity);
    }
    
    // Metadata

    [Fact]
    public void UpdateMetadata_ValidMetadata_UpdatesMetadata()
    {
        var resource = CreateValidResource();

        var metadata = new Dictionary<string, object>
        {
            ["brand"] = "Toyota",
            ["model"] = "Camry",
            ["year"] = 2024
        };
        
        resource.UpdateMetadata(metadata);
        
        Assert.Same(metadata, resource.Metadata);
    }


    [Fact]
    public void UpdateMetadata_Null_ClearsMetadata()
    {
        var metadata = new Dictionary<string, object>
        {
            ["brand"] = "Toyota"
        };

        var resource = new Resource(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Toyota Camry",
            metadata: metadata);
        
        resource.UpdateMetadata(null);
        
        Assert.Null(resource.Metadata);
    }

    // Deactivate

    [Fact]
    public void Deactivate_ActiveResource_DeactivatesResource()
    {
        var resource = CreateValidResource();
        
        resource.Deactivate();
        
        Assert.False(resource.IsActive);
    }


    [Fact]
    public void Deactivate_AlreadyInactiveResource_RemainsInactive()
    {
        var resource = CreateValidResource();

        resource.Deactivate();
        resource.Deactivate();
        
        Assert.False(resource.IsActive);
    }


    [Fact]
    public void Activate_InactiveResource_ActivatesResource()
    {
        var resource = CreateValidResource();

        resource.Deactivate();
        resource.Activate();
        
        Assert.True(resource.IsActive);
    }

    // Activate
    [Fact]
    public void Activate_AlreadyActiveResource_RemainsActive()
    {
        var resource = CreateValidResource();
        
        resource.Activate();
        
        Assert.True(resource.IsActive);
    }

    
    // AddOffer

    [Fact]
    public void AddOffer_ValidOffer_AddsOffer()
    {
        var resource = CreateValidResource();
        var offer = CreateValidOffer();
        
        resource.AddOffer(offer);
        
        Assert.Single(resource.Offers);
        Assert.Contains(offer, resource.Offers);
    }


    [Fact]
    public void AddOffer_NullOffer_ThrowsArgumentNullException()
    {
        var resource = CreateValidResource();
        
        var action = () => resource.AddOffer(null!);
        
        var exception = Assert.Throws<ArgumentNullException>(action);

        Assert.Equal("offer", exception.ParamName);
    }


    [Fact]
    public void AddOffer_DuplicateActiveOfferName_ThrowsInvalidOperationException()
    {
        var resource = CreateValidResource();

        var firstOffer = CreateValidOffer("Почасовая аренда");
        var secondOffer = CreateValidOffer("Почасовая аренда");

        resource.AddOffer(firstOffer);
        
        var action = () => resource.AddOffer(secondOffer);
        
        var exception = Assert.Throws<InvalidOperationException>(action);

        Assert.Equal("У ресурса уже существует услуга с таким названием.", exception.Message);

        Assert.Single(resource.Offers);
    }
    
    [Fact]
    public void AddOffer_DuplicateActiveOfferNameIgnoringCase_ThrowsInvalidOperationException()
    {
        var resource = CreateValidResource();

        var firstOffer = CreateValidOffer("Почасовая аренда");
        var secondOffer = CreateValidOffer("ПОЧАСОВАЯ АРЕНДА");

        resource.AddOffer(firstOffer);
        
        var action = () => resource.AddOffer(secondOffer);
        
        Assert.Throws<InvalidOperationException>(action);

        Assert.Single(resource.Offers);
    }


    [Fact]
    public void AddOffer_DifferentNames_AddsBothOffers()
    {
        var resource = CreateValidResource();

        var firstOffer = CreateValidOffer("Почасовая аренда");
        var secondOffer = CreateValidOffer("Суточная аренда");
        
        resource.AddOffer(firstOffer);
        resource.AddOffer(secondOffer);
        
        Assert.Equal(2, resource.Offers.Count);
    }


    [Fact]
    public void AddOffer_SameNameAsInactiveOffer_AddsOffer()
    {
        var resource = CreateValidResource();

        var inactiveOffer = CreateValidOffer("Почасовая аренда");
        inactiveOffer.Deactivate();

        var newOffer = CreateValidOffer("Почасовая аренда");

        resource.AddOffer(inactiveOffer);
        
        resource.AddOffer(newOffer);
        
        Assert.Equal(2, resource.Offers.Count);
        Assert.Contains(inactiveOffer, resource.Offers);
        Assert.Contains(newOffer, resource.Offers);
    }

    
    // AddScheduleOverride

    [Fact]
    public void AddScheduleOverride_ValidOverride_AddsOverride()
    {
        var resource = CreateValidResource();

        var scheduleOverride = CreateScheduleOverride(resource.Id);
        
        resource.AddScheduleOverride(scheduleOverride);
        
        Assert.Single(resource.ScheduleOverrides);
        Assert.Contains(scheduleOverride, resource.ScheduleOverrides);
    }
    
    [Fact]
    public void AddScheduleOverride_NullOverride_ThrowsArgumentNullException()
    {
        var resource = CreateValidResource();
        
        var action = () => resource.AddScheduleOverride(null!);
        
        var exception = Assert.Throws<ArgumentNullException>(action);

        Assert.Equal("scheduleOverride", exception.ParamName);
    }
    
    [Fact]
    public void AddScheduleOverride_DifferentResource_ThrowsInvalidOperationException()
    {
        var resource = CreateValidResource();

        var scheduleOverride = CreateScheduleOverride(Guid.NewGuid());
        
        var action = () => resource.AddScheduleOverride(scheduleOverride);
        
        var exception = Assert.Throws<InvalidOperationException>(action);

        Assert.Equal("Исключение расписания относится к другому ресурсу.", exception.Message);

        Assert.Empty(resource.ScheduleOverrides);
    }
    
    [Fact]
    public void AddScheduleOverride_OverlappingPeriod_ThrowsInvalidOperationException()
    {
        var resource = CreateValidResource();

        var firstOverride =
            CreateScheduleOverride(
                resource.Id,
                startHour: 10,
                endHour: 12);

        var overlappingOverride =
            CreateScheduleOverride(
                resource.Id,
                startHour: 11,
                endHour: 13);

        resource.AddScheduleOverride(firstOverride);
        
        var action = () => resource.AddScheduleOverride(overlappingOverride);
        
        var exception = Assert.Throws<InvalidOperationException>(action);

        Assert.Equal("Период исключения расписания пересекается с уже существующим.", exception.Message);

        Assert.Single(resource.ScheduleOverrides);
    }
    
    [Fact]
    public void AddScheduleOverride_OverlappingAtStart_ThrowsInvalidOperationException()
    {
        var resource = CreateValidResource();

        var firstOverride = CreateScheduleOverride(resource.Id, startHour: 10, endHour: 12);

        var overlappingOverride = CreateScheduleOverride(resource.Id, startHour: 9, endHour: 11);

        resource.AddScheduleOverride(firstOverride);
        
        var action = () => resource.AddScheduleOverride(overlappingOverride);
        
        Assert.Throws<InvalidOperationException>(action);
    }


    [Fact]
    public void AddScheduleOverride_NewPeriodInsideExisting_ThrowsInvalidOperationException()
    {
        var resource = CreateValidResource();

        var firstOverride = CreateScheduleOverride(resource.Id, startHour: 9, endHour: 13);

        var overlappingOverride = CreateScheduleOverride(resource.Id, startHour: 10, endHour: 12);

        resource.AddScheduleOverride(firstOverride);
        
        var action = () => resource.AddScheduleOverride(overlappingOverride);
        
        Assert.Throws<InvalidOperationException>(action);
    }


    [Fact]
    public void AddScheduleOverride_NewPeriodContainsExisting_ThrowsInvalidOperationException()
    {
        var resource = CreateValidResource();

        var firstOverride = CreateScheduleOverride(resource.Id, startHour: 10, endHour: 12);

        var overlappingOverride = CreateScheduleOverride(resource.Id, startHour: 9, endHour: 13);

        resource.AddScheduleOverride(firstOverride);
        
        var action = () => resource.AddScheduleOverride(overlappingOverride);
        
        Assert.Throws<InvalidOperationException>(action);
    }


    [Fact]
    public void AddScheduleOverride_SamePeriod_ThrowsInvalidOperationException()
    {
        var resource = CreateValidResource();

        var firstOverride = CreateScheduleOverride(resource.Id, startHour: 10, endHour: 12);

        var duplicateOverride = CreateScheduleOverride(resource.Id, startHour: 10, endHour: 12);

        resource.AddScheduleOverride(firstOverride);
        
        var action = () => resource.AddScheduleOverride(duplicateOverride);
        
        Assert.Throws<InvalidOperationException>(action);
    }
    
    [Fact]
    public void AddScheduleOverride_AdjacentPeriods_AddsBothOverrides()
    {
        var resource = CreateValidResource();

        var firstOverride = CreateScheduleOverride(resource.Id, startHour: 10, endHour: 12);

        var secondOverride = CreateScheduleOverride(resource.Id, startHour: 12, endHour: 14);

        resource.AddScheduleOverride(firstOverride);
        resource.AddScheduleOverride(secondOverride);
        
        Assert.Equal(2, resource.ScheduleOverrides.Count);
    }
    
    [Fact]
    public void AddScheduleOverride_OverlapsInactiveOverride_AddsOverride()
    {
        var resource = CreateValidResource();

        var inactiveOverride = CreateScheduleOverride(resource.Id, startHour: 10, endHour: 12);

        inactiveOverride.Deactivate();

        var newOverride = CreateScheduleOverride(resource.Id, startHour: 11, endHour: 13);

        resource.AddScheduleOverride(inactiveOverride);
        resource.AddScheduleOverride(newOverride);
        
        Assert.Equal(2, resource.ScheduleOverrides.Count);
    }
}