using TestService.Domain.Events;

namespace TestService.UnitTests.Domain.Events;

public class ProductEventTests
{
    [Fact]
    public void ProductCreatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var domainEvent = new ProductCreatedEvent(aggregateId, id, createdAt, updatedAt);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.ProductId.Should().Be(aggregateId);
domainEvent.Id.Should().Be(id);
        domainEvent.CreatedAt.Should().Be(createdAt);
        domainEvent.UpdatedAt.Should().Be(updatedAt);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void ProductCreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
Guid id,
        DateTime createdAt,
        DateTime updatedAt
    )
    {
        // Act
        var domainEvent = new ProductCreatedEvent(aggregateId, id, createdAt, updatedAt);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.ProductId.Should().Be(aggregateId);
    }
}