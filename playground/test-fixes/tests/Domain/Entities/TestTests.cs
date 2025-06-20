using TestService.Domain.Entities;
using TestService.Domain.Events;
using TestService.UnitTests.Builders;

namespace TestService.UnitTests.Domain.Entities;

public class TestTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEntity()
    {
        // Arrange
var id = Guid.NewGuid();
        var name = "Test Value";
        var description = "Test Value";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var entity = new Test(id, name, description, createdAt, updatedAt);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.Id.Should().Be(Guid.NewGuid());
        entity.Name.Should().Be("Test Value");
        entity.Description.Should().Be("Test Value");
        entity.CreatedAt.Should().Be(DateTime.UtcNow);
        entity.UpdatedAt.Should().Be(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_ShouldRaiseTestCreatedEvent()
    {
        // Arrange
var id = Guid.NewGuid();
        var name = "Test Value";
        var description = "Test Value";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var entity = new Test(id, name, description, createdAt, updatedAt);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TestCreatedEvent>();
    }


    [Fact]
    public void Create_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new TestBuilder().Build();
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
        var entity = new TestBuilder().Build();
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
        var entity = new TestBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.Delete();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }

    [Theory]
    [AutoData]
    public void Constructor_WithBuilder_ShouldCreateValidEntity(TestBuilder builder)
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
        var entity = new TestBuilder().Build();
        entity.DomainEvents.Should().NotBeEmpty();

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }
}