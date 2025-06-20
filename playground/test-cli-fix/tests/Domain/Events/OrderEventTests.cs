using TestService.Domain.Events;

namespace TestService.UnitTests.Domain.Events;

public class OrderEventTests
{
    [Fact]
    public void OrderCreatedEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
var customerId = Guid.NewGuid();
        var totalAmount = 123.45m;
        var status = default(OrderStatus);

        // Act
        var domainEvent = new OrderCreatedEvent(aggregateId, customerId, totalAmount, status);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.OrderId.Should().Be(aggregateId);
domainEvent.CustomerId.Should().Be(customerId);
        domainEvent.TotalAmount.Should().Be(totalAmount);
        domainEvent.Status.Should().Be(status);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void OrderCreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
Guid customerId,
        decimal totalAmount,
        OrderStatus status
    )
    {
        // Act
        var domainEvent = new OrderCreatedEvent(aggregateId, customerId, totalAmount, status);

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.OrderId.Should().Be(aggregateId);
    }
}