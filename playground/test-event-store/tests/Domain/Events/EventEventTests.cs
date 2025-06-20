using EventStoreService.Domain.Events;

namespace EventStoreService.UnitTests.Domain.Events;

public class EventEventTests
{
    [Fact]
    public void EventCreatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
var id = Guid.NewGuid();
        var name = "Test Value";
        var description = "Test Value";

        // Act
        var domainEvent = new EventCreatedEvent(aggregateId, id, name, description);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.EventId.Should().Be(aggregateId);
domainEvent.Id.Should().Be(id);
        domainEvent.Name.Should().Be(name);
        domainEvent.Description.Should().Be(description);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void EventCreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
Guid id,
        string name,
        string description
    )
    {
        // Act
        var domainEvent = new EventCreatedEvent(aggregateId, id, name, description);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.EventId.Should().Be(aggregateId);
    }
}