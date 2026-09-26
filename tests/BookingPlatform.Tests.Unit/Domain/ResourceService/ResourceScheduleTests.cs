using BookingPlatform.ResourceService.Domain.Entities;

namespace BookingPlatform.Tests.Unit.Domain.ResourceService;

public class ResourceScheduleTests
{
    [Fact]
    public void Constructor_ValidData_CreatesSchedule()
    {
        var id = Guid.NewGuid();

        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(18, 0);
        
        var schedule = new ResourceSchedule(id, DayOfWeek.Monday, startTime, endTime);
        
        Assert.Equal(id, schedule.Id);
        Assert.Equal(DayOfWeek.Monday, schedule.DayOfWeek);
        Assert.Equal(startTime, schedule.StartTime);
        Assert.Equal(endTime, schedule.EndTime);
        Assert.True(schedule.IsActive);
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsArgumentException()
    {
        var action = () => new ResourceSchedule(
            Guid.Empty, 
            DayOfWeek.Monday, 
            new TimeOnly(9, 0), 
            new TimeOnly(18, 0)
            );
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_EndTimeBeforeStartTime_ThrowsArgumentException()
    {
        var action = () => new ResourceSchedule(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            new TimeOnly(18, 0),
            new TimeOnly(9, 0)
            );
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("endTime", exception.ParamName);
    }

    [Fact]
    public void Constructor_StartTimeEqualsEndTime_ThrowsArgumentException()
    {
        var action = () => new ResourceSchedule(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(9, 0)
            );
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("endTime", exception.ParamName);
    }

    [Fact]
    public void Update_ValidData_UpdatesSchedule()
    {
        var schedule = new ResourceSchedule(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0));
        
        schedule.Update(
            DayOfWeek.Tuesday,
            new TimeOnly(10, 0),
            new TimeOnly(19, 0));
        
        Assert.Equal(DayOfWeek.Tuesday, schedule.DayOfWeek);
        Assert.Equal(new TimeOnly(10, 0), schedule.StartTime);
        Assert.Equal(new TimeOnly(19, 0), schedule.EndTime);
    }

    [Fact]
    public void Update_EndTimeBeforeStartTime_ThrowsArgumentException()
    {
        var schedule = new ResourceSchedule(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0));
        
        var action = () => schedule.Update(
            DayOfWeek.Tuesday,
            new TimeOnly(18, 0),
            new TimeOnly(9, 0));
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("endTime", exception.ParamName);
    }

    [Fact]
    public void Update_StartTimeEqualsEndTime_ThrowsArgumentException()
    {
        var schedule = new ResourceSchedule(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0));
        
        var action = () => schedule.Update(
            DayOfWeek.Tuesday,
            new TimeOnly(10, 0),
            new TimeOnly(10, 0));
        
        var exception = Assert.Throws<ArgumentException>(action);

        Assert.Equal("endTime", exception.ParamName);
    }

    [Fact]
    public void Deactivate_ActiveSchedule_DeactivatesSchedule()
    {
        var schedule = CreateValidSchedule();
        
        schedule.Deactivate();
        
        Assert.False(schedule.IsActive);
    }

    [Fact]
    public void Activate_InactiveSchedule_ActivatesSchedule()
    {
        var schedule = CreateValidSchedule();

        schedule.Deactivate();
        schedule.Activate();
        
        Assert.True(schedule.IsActive);
    }

    [Fact]
    public void Deactivate_AlreadyInactiveSchedule_RemainsInactive()
    {
        var schedule = CreateValidSchedule();

        schedule.Deactivate();
        schedule.Deactivate();
        
        Assert.False(schedule.IsActive);
    }

    [Fact]
    public void Activate_AlreadyActiveSchedule_RemainsActive()
    {
        var schedule = CreateValidSchedule();
        
        schedule.Activate();
        
        Assert.True(schedule.IsActive);
    }

    private static ResourceSchedule CreateValidSchedule()
    {
        return new ResourceSchedule(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(18, 0));
    }
}