using ReadModelService.Domain.Entities;
using ReadModelService.Domain.Events;
using ReadModelService.UnitTests.Builders;

namespace ReadModelService.UnitTests.Domain.Entities;

public class ProductTests
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
        var entity = new Product(id, name, description);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.Id.Should().Be(Guid.NewGuid());
        entity.Name.Should().Be("Test Value");
        entity.Description.Should().Be("Test Value");
    }

    [Fact]
    public void Constructor_ShouldRaiseProductCreatedEvent()
    {
        // Arrange
var id = Guid.NewGuid();
        var name = "Test Value";
        var description = "Test Value";

        // Act
        var entity = new Product(id, name, description);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedEvent>();
    }


    [Fact]
    public void Create_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new ProductBuilder().Build();
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
        var entity = new ProductBuilder().Build();
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
        var entity = new ProductBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.Delete();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }

    [Theory]
    [AutoData]
    public void Constructor_WithBuilder_ShouldCreateValidEntity(ProductBuilder builder)
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
        var entity = new ProductBuilder().Build();
        entity.DomainEvents.Should().NotBeEmpty();

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }
}