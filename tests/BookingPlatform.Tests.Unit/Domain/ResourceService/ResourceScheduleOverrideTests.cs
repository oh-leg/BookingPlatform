using BookingPlatform.ResourceService.Domain.Entities;
using BookingPlatform.ResourceService.Domain.Enums;

namespace BookingPlatform.Test.Unit.Domain.ResourceService
{
    public class ResourceScheduleOverrideTests
    {
        private static ResourceScheduleOverride CreateValidOverride(
            Guid? resourceId = null)
        {
            return new ResourceScheduleOverride(
                id: Guid.NewGuid(),
                resourceId: resourceId ?? Guid.NewGuid(),
                startAt: new DateTimeOffset(
                    2026, 8, 16,
                    10, 0, 0,
                    TimeSpan.FromHours(3)),
                endAt: new DateTimeOffset(
                    2026, 8, 16,
                    12, 0, 0,
                    TimeSpan.FromHours(3)),
                type: ScheduleOverrideType.Unavailable,
                reason: "Техническое обслуживание");
        }
        
        // Constructor

        [Fact]
        public void Constructor_ValidData_CreatesScheduleOverride()
        {
            var id = Guid.NewGuid();
            var resourceId = Guid.NewGuid();

            var startAt = new DateTimeOffset(2026, 8, 16, 10, 0, 0, TimeSpan.FromHours(3));
            var endAt = new DateTimeOffset(2026, 8, 16, 12, 0, 0, TimeSpan.FromHours(3));
            
            var scheduleOverride = new ResourceScheduleOverride(
                id,
                resourceId,
                startAt,
                endAt,
                ScheduleOverrideType.Unavailable,
                "  Техническое обслуживание  ");
            
            Assert.Equal(id, scheduleOverride.Id);
            Assert.Equal(resourceId, scheduleOverride.ResourceId);

            Assert.Equal(startAt, scheduleOverride.StartAt);
            Assert.Equal(endAt, scheduleOverride.EndAt);

            Assert.Equal(ScheduleOverrideType.Unavailable, scheduleOverride.Type);

            Assert.Equal("Техническое обслуживание", scheduleOverride.Reason);

            Assert.True(scheduleOverride.IsActive);

            Assert.True(scheduleOverride.CreatedAt <= DateTimeOffset.UtcNow);
        }

        [Fact]
        public void Constructor_EmptyId_ThrowsArgumentException()
        {
            var action = () => new ResourceScheduleOverride(
                Guid.Empty,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1),
                ScheduleOverrideType.Unavailable);
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("id", exception.ParamName);
        }

        [Fact]
        public void Constructor_EmptyResourceId_ThrowsArgumentException()
        {
            var action = () => new ResourceScheduleOverride(
                Guid.NewGuid(),
                Guid.Empty,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1),
                ScheduleOverrideType.Unavailable);
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("resourceId", exception.ParamName);
        }

        [Fact]
        public void Constructor_EndAtBeforeStartAt_ThrowsArgumentException()
        {
            var startAt = DateTimeOffset.UtcNow;
            var endAt = startAt.AddHours(-1);
            
            var action = () => new ResourceScheduleOverride(
                Guid.NewGuid(),
                Guid.NewGuid(),
                startAt,
                endAt,
                ScheduleOverrideType.Unavailable);
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("endAt", exception.ParamName);
        }

        [Fact]
        public void Constructor_EndAtEqualsStartAt_ThrowsArgumentException()
        {
            var dateTime = DateTimeOffset.UtcNow;
            
            var action = () => new ResourceScheduleOverride(
                Guid.NewGuid(),
                Guid.NewGuid(),
                dateTime,
                dateTime,
                ScheduleOverrideType.Unavailable);
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("endAt", exception.ParamName);
        }

        [Fact]
        public void Constructor_InvalidType_ThrowsArgumentException()
        {
            var invalidType = (ScheduleOverrideType)999;
            
            var action = () => new ResourceScheduleOverride(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1),
                invalidType);
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("type", exception.ParamName);
        }

        [Fact]
        public void Constructor_NullReason_CreatesOverride()
        {
            var scheduleOverride = new ResourceScheduleOverride(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1),
                ScheduleOverrideType.Unavailable,
                null);
            
            Assert.Null(scheduleOverride.Reason);
        }

        [Fact]
        public void Constructor_ReasonWithSpaces_TrimsReason()
        {
            var scheduleOverride = new ResourceScheduleOverride(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1),
                ScheduleOverrideType.Unavailable,
                "  Техническое обслуживание  ");
            
            Assert.Equal("Техническое обслуживание", scheduleOverride.Reason);
        }

        
        // Update

        [Fact]
        public void Update_ValidData_UpdatesScheduleOverride()
        {
            var scheduleOverride = CreateValidOverride();

            var startAt = new DateTimeOffset(2026, 8, 17, 14, 0, 0, TimeSpan.FromHours(3));
            var endAt = new DateTimeOffset(2026, 8, 17, 16, 0, 0, TimeSpan.FromHours(3));
            
            scheduleOverride.Update(
                startAt,
                endAt,
                ScheduleOverrideType.Available,
                "  Рабочее время  ");
            
            Assert.Equal(startAt, scheduleOverride.StartAt);
            Assert.Equal(endAt, scheduleOverride.EndAt);

            Assert.Equal(ScheduleOverrideType.Available, scheduleOverride.Type);

            Assert.Equal("Рабочее время", scheduleOverride.Reason);
        }

        [Fact]
        public void Update_EndAtBeforeStartAt_ThrowsArgumentException()
        {
            var scheduleOverride = CreateValidOverride();

            var startAt = DateTimeOffset.UtcNow;
            var endAt = startAt.AddHours(-1);
            
            var action = () => scheduleOverride.Update(startAt, endAt, ScheduleOverrideType.Unavailable);
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("endAt", exception.ParamName);
        }

        [Fact]
        public void Update_EndAtEqualsStartAt_ThrowsArgumentException()
        {
            var scheduleOverride = CreateValidOverride();

            var dateTime = DateTimeOffset.UtcNow;
            
            var action = () => scheduleOverride.Update(dateTime, dateTime, ScheduleOverrideType.Unavailable);
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("endAt", exception.ParamName);
        }

        [Fact]
        public void Update_InvalidType_ThrowsArgumentException()
        {
            var scheduleOverride = CreateValidOverride();

            var invalidType = (ScheduleOverrideType)999;
            
            var action = () => scheduleOverride.Update(
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1),
                invalidType);
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("type", exception.ParamName);
        }

        [Fact]
        public void Update_NullReason_ClearsReason()
        {
            var scheduleOverride = CreateValidOverride();
            
            scheduleOverride.Update(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), ScheduleOverrideType.Unavailable, null);
            
            Assert.Null(scheduleOverride.Reason);
        }

        [Fact]
        public void Update_InvalidPeriod_DoesNotChangeExistingData()
        {
            var scheduleOverride = CreateValidOverride();

            var originalStartAt = scheduleOverride.StartAt;
            var originalEndAt = scheduleOverride.EndAt;
            var originalType = scheduleOverride.Type;
            var originalReason = scheduleOverride.Reason;

            var startAt = DateTimeOffset.UtcNow;
            var endAt = startAt.AddHours(-1);
            
            var action = () => scheduleOverride.Update(startAt, endAt, ScheduleOverrideType.Available, "Новое описание");
            
            Assert.Throws<ArgumentException>(action);

            Assert.Equal(originalStartAt, scheduleOverride.StartAt);
            Assert.Equal(originalEndAt, scheduleOverride.EndAt);
            Assert.Equal(originalType, scheduleOverride.Type);
            Assert.Equal(originalReason, scheduleOverride.Reason);
        }

        // Deactivate

        [Fact]
        public void Activate_InactiveOverride_ActivatesOverride()
        {
            var scheduleOverride = CreateValidOverride();

            scheduleOverride.Deactivate();
            scheduleOverride.Activate();
            
            Assert.True(scheduleOverride.IsActive);
        }

        [Fact]
        public void Activate_AlreadyActiveOverride_RemainsActive()
        {
            var scheduleOverride = CreateValidOverride();
            
            scheduleOverride.Activate();
            
            Assert.True(scheduleOverride.IsActive);
        }

        [Fact]
        public void Deactivate_ActiveOverride_DeactivatesOverride()
        {
            var scheduleOverride = CreateValidOverride();
            
            scheduleOverride.Deactivate();
            
            Assert.False(scheduleOverride.IsActive);
        }

        [Fact]
        public void Deactivate_AlreadyInactiveOverride_RemainsInactive()
        {
            var scheduleOverride = CreateValidOverride();

            scheduleOverride.Deactivate();
            scheduleOverride.Deactivate();
            
            Assert.False(scheduleOverride.IsActive);
        }
    }
}