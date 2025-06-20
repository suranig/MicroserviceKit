using TestService.Domain.Events;

namespace TestService.UnitTests.Domain.Events;

public class TestEventTests
{
    [Fact]
    public void TestCreatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
var id = Guid.NewGuid();
        var name = "Test Value";
        var description = "Test Value";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var domainEvent = new TestCreatedEvent(aggregateId, id, name, description, createdAt, updatedAt);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.TestId.Should().Be(aggregateId);
domainEvent.Id.Should().Be(id);
        domainEvent.Name.Should().Be(name);
        domainEvent.Description.Should().Be(description);
        domainEvent.CreatedAt.Should().Be(createdAt);
        domainEvent.UpdatedAt.Should().Be(updatedAt);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void TestCreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
Guid id,
        string name,
        string description,
        DateTime createdAt,
        DateTime updatedAt
    )
    {
        // Act
        var domainEvent = new TestCreatedEvent(aggregateId, id, name, description, createdAt, updatedAt);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.TestId.Should().Be(aggregateId);
    }
}