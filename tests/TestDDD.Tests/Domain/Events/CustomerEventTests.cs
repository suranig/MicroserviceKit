using ECommerce.OrderService.Domain.Events;

namespace ECommerce.OrderService.UnitTests.Domain.Events;

public class CustomerEventTests
{
    [Fact]
    public void CustomerCreatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
var email = "Test Value";
        var name = "Test Value";

        // Act
        var domainEvent = new CustomerCreatedEvent(aggregateId, email, name);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.CustomerId.Should().Be(aggregateId);
domainEvent.Email.Should().Be(email);
        domainEvent.Name.Should().Be(name);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void CustomerCreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
string email,
        string name
    )
    {
        // Act
        var domainEvent = new CustomerCreatedEvent(aggregateId, email, name);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.CustomerId.Should().Be(aggregateId);
    }
}