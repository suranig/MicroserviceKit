using SimpleService.Domain.Events;

namespace SimpleService.UnitTests.Domain.Events;

public class UserEventTests
{
    [Fact]
    public void UserCreatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
var id = Guid.NewGuid();
        var name = "Test Value";
        var description = "Test Value";

        // Act
        var domainEvent = new UserCreatedEvent(aggregateId, id, name, description);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.UserId.Should().Be(aggregateId);
domainEvent.Id.Should().Be(id);
        domainEvent.Name.Should().Be(name);
        domainEvent.Description.Should().Be(description);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void UserCreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
Guid id,
        string name,
        string description
    )
    {
        // Act
        var domainEvent = new UserCreatedEvent(aggregateId, id, name, description);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.UserId.Should().Be(aggregateId);
    }
}