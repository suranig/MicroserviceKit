using Company.TestService.Domain.Events;

namespace Company.TestService.UnitTests.Domain.Events;

public class ItemEventTests
{
    [Fact]
    public void ItemCreatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
var title = "Test Value";
        var isCompleted = true;

        // Act
        var domainEvent = new ItemCreatedEvent(aggregateId, title, isCompleted);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.ItemId.Should().Be(aggregateId);
domainEvent.Title.Should().Be(title);
        domainEvent.IsCompleted.Should().Be(isCompleted);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void ItemCreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
string title,
        bool isCompleted
    )
    {
        // Act
        var domainEvent = new ItemCreatedEvent(aggregateId, title, isCompleted);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.ItemId.Should().Be(aggregateId);
    }
}