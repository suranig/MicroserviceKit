using SimpleService.Domain.Entities;
using SimpleService.Domain.Events;
using SimpleService.UnitTests.Builders;

namespace SimpleService.UnitTests.Domain.Entities;

public class UserTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEntity()
    {
        // Arrange
var id = Guid.NewGuid();
        var name = "Test Value";
        var description = "Test Value";

        // Act
        var entity = new User(id, name, description);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.Id.Should().Be(Guid.NewGuid());
        entity.Name.Should().Be("Test Value");
        entity.Description.Should().Be("Test Value");
    }

    [Fact]
    public void Constructor_ShouldRaiseUserCreatedEvent()
    {
        // Arrange
var id = Guid.NewGuid();
        var name = "Test Value";
        var description = "Test Value";

        // Act
        var entity = new User(id, name, description);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedEvent>();
    }


    [Fact]
    public void Create_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new UserBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.Create();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }
    [Fact]
    public void Update_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new UserBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.Update();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }
    [Fact]
    public void Delete_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new UserBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.Delete();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }

    [Theory]
    [AutoData]
    public void Constructor_WithBuilder_ShouldCreateValidEntity(UserBuilder builder)
    {
        // Act
        var entity = builder.Build();

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.DomainEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var entity = new UserBuilder().Build();
        entity.DomainEvents.Should().NotBeEmpty();

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }
}