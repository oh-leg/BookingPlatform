using BookingPlatform.ResourceService.Domain.Entities;

namespace BookingPlatform.Test.Unit.Domain.ResourceService
{
    public class ResourceTypeTests
    {
        private static ResourceType CreateValidResourceType()
        {
            return new ResourceType(
                id: Guid.NewGuid(),
                name: "Автомобили",
                description: "Тип ресурсов для автомобилей"
            );
        }
        
        // Constructor

        [Fact]
        public void Constructor_ValidData_CreatesResourceType()
        {
            var id = Guid.NewGuid();
            
            var resourceType = new ResourceType(
                id,
                "  Автомобили  ",
                "  Тип ресурсов для автомобилей  ");
            
            Assert.Equal(id, resourceType.Id);
            Assert.Equal("Автомобили", resourceType.Name);
            Assert.Equal("Тип ресурсов для автомобилей", resourceType.Description);

            Assert.True(resourceType.IsActive);
            Assert.True(resourceType.CreatedAt <= DateTimeOffset.UtcNow);
        }

        [Fact]
        public void Constructor_EmptyId_ThrowsArgumentException()
        {
            var id = Guid.Empty;
            
            var action = () => new ResourceType(id, "Автомобили");
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("id", exception.ParamName);
        }

        [Fact]
        public void Constructor_EmptyName_ThrowsArgumentException()
        {
            var action = () => new ResourceType(Guid.NewGuid(), "");
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public void Constructor_WhitespaceName_ThrowsArgumentException()
        {
            var action = () => new ResourceType(Guid.NewGuid(), "   ");
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public void Constructor_NameWithSpaces_TrimsName()
        {
            var resourceType = new ResourceType(Guid.NewGuid(), "  Автомобили  ");
            
            Assert.Equal("Автомобили", resourceType.Name);
        }

        [Fact]
        public void Constructor_NullDescription_CreatesResourceType()
        {
            var resourceType = new ResourceType(Guid.NewGuid(), "Автомобили", null);
            
            Assert.Null(resourceType.Description);
        }

        [Fact]
        public void Constructor_DescriptionWithSpaces_TrimsDescription()
        {
            var resourceType = new ResourceType(Guid.NewGuid(), "Автомобили", "  Тип ресурсов  ");
            
            Assert.Equal("Тип ресурсов", resourceType.Description);
        }
        
        // Rename

        [Fact]
        public void Rename_ValidName_ChangesName()
        {
            var resourceType = CreateValidResourceType();
            
            resourceType.Rename("  Помещения  ");
            
            Assert.Equal("Помещения", resourceType.Name);
        }

        [Fact]
        public void Rename_EmptyName_ThrowsArgumentException()
        {
            var resourceType = CreateValidResourceType();
            
            var action = () => resourceType.Rename("");
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public void Rename_WhitespaceName_ThrowsArgumentException()
        {
            var resourceType = CreateValidResourceType();
            
            var action = () => resourceType.Rename("   ");
            
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public void Rename_InvalidName_DoesNotChangeExistingName()
        {
            var resourceType = CreateValidResourceType();
            var originalName = resourceType.Name;
            
            var action = () => resourceType.Rename("   ");
            
            Assert.Throws<ArgumentException>(action);
            Assert.Equal(originalName, resourceType.Name);
        }
        
        // Description

        [Fact]
        public void UpdateDescription_ValidDescription_UpdatesDescription()
        {
            var resourceType = CreateValidResourceType();
            
            resourceType.UpdateDescription("  Новый тип ресурсов  ");
            
            Assert.Equal("Новый тип ресурсов", resourceType.Description);
        }

        [Fact]
        public void UpdateDescription_Null_ClearsDescription()
        {
            var resourceType = CreateValidResourceType();
            
            resourceType.UpdateDescription(null);
            
            Assert.Null(resourceType.Description);
        }

        [Fact]
        public void UpdateDescription_EmptyDescription_SetsEmptyDescription()
        {
            var resourceType = CreateValidResourceType();
            
            resourceType.UpdateDescription("");
            
            Assert.Equal("", resourceType.Description);
        }
        // InActivate-Deactivate

        [Fact]
        public void Activate_InactiveResourceType_ActivatesResourceType()
        {
            var resourceType = CreateValidResourceType();

            resourceType.Deactivate();
            
            resourceType.Activate();
            
            Assert.True(resourceType.IsActive);
        }

        [Fact]
        public void Activate_AlreadyActiveResourceType_RemainsActive()
        {
            var resourceType = CreateValidResourceType();
            
            resourceType.Activate();
            
            Assert.True(resourceType.IsActive);
        }
        
        // Deactivate

        [Fact]
        public void Deactivate_ActiveResourceType_DeactivatesResourceType()
        {
            var resourceType = CreateValidResourceType();
            
            resourceType.Deactivate();
            
            Assert.False(resourceType.IsActive);
        }

        [Fact]
        public void Deactivate_AlreadyInactiveResourceType_RemainsInactive()
        {
            var resourceType = CreateValidResourceType();

            resourceType.Deactivate();
            
            resourceType.Deactivate();
            
            Assert.False(resourceType.IsActive);
        }
    }
}