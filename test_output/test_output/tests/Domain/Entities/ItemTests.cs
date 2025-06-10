using Company.TestService.Domain.Entities;
using Company.TestService.Domain.Events;
using Company.TestService.UnitTests.Builders;

namespace Company.TestService.UnitTests.Domain.Entities;

public class ItemTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEntity()
    {
        // Arrange
var title = "Test Value";
        var isCompleted = true;

        // Act
        var entity = new Item(title, isCompleted);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.Title.Should().Be("Test Value");
        entity.IsCompleted.Should().Be(true);
    }

    [Fact]
    public void Constructor_ShouldRaiseItemCreatedEvent()
    {
        // Arrange
var title = "Test Value";
        var isCompleted = true;

        // Act
        var entity = new Item(title, isCompleted);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ItemCreatedEvent>();
    }


    [Fact]
    public void Create_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new ItemBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.Create();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }
    [Fact]
    public void MarkComplete_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new ItemBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.MarkComplete();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }

    [Theory]
    [AutoData]
    public void Constructor_WithBuilder_ShouldCreateValidEntity(ItemBuilder builder)
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
        var entity = new ItemBuilder().Build();
        entity.DomainEvents.Should().NotBeEmpty();

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }
}