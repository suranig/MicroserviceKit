using MyApp.UserService.Domain.Events;

namespace MyApp.UserService.UnitTests.Domain.Events;

public class UserEventTests
{
    [Fact]
    public void UserCreatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
var email = "Test Value";
        var name = "Test Value";

        // Act
        var domainEvent = new UserCreatedEvent(aggregateId, email, name);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.UserId.Should().Be(aggregateId);
domainEvent.Email.Should().Be(email);
        domainEvent.Name.Should().Be(name);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void UserCreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
string email,
        string name
    )
    {
        // Act
        var domainEvent = new UserCreatedEvent(aggregateId, email, name);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.UserId.Should().Be(aggregateId);
    }
}