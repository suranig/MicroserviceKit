using ReadModelService.Domain.Events;

namespace ReadModelService.UnitTests.Domain.Events;

public class ProductEventTests
{
    [Fact]
    public void ProductCreatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
var id = Guid.NewGuid();
        var name = "Test Value";
        var description = "Test Value";

        // Act
        var domainEvent = new ProductCreatedEvent(aggregateId, id, name, description);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.ProductId.Should().Be(aggregateId);
domainEvent.Id.Should().Be(id);
        domainEvent.Name.Should().Be(name);
        domainEvent.Description.Should().Be(description);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void ProductCreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
Guid id,
        string name,
        string description
    )
    {
        // Act
        var domainEvent = new ProductCreatedEvent(aggregateId, id, name, description);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.ProductId.Should().Be(aggregateId);
    }
}