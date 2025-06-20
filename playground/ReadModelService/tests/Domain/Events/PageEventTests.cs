using ReadModelService.Domain.Events;

namespace ReadModelService.UnitTests.Domain.Events;

public class PageEventTests
{
    [Fact]
    public void PageCreatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var domainEvent = new PageCreatedEvent(aggregateId, id, createdAt, updatedAt);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.PageId.Should().Be(aggregateId);
domainEvent.Id.Should().Be(id);
        domainEvent.CreatedAt.Should().Be(createdAt);
        domainEvent.UpdatedAt.Should().Be(updatedAt);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void PageCreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
Guid id,
        DateTime createdAt,
        DateTime updatedAt
    )
    {
        // Act
        var domainEvent = new PageCreatedEvent(aggregateId, id, createdAt, updatedAt);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.PageId.Should().Be(aggregateId);
    }
}