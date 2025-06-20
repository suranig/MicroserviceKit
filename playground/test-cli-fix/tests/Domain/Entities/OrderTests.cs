using TestService.Domain.Entities;
using TestService.Domain.Events;
using TestService.UnitTests.Builders;

namespace TestService.UnitTests.Domain.Entities;

public class OrderTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEntity()
    {
        // Arrange
var customerId = Guid.NewGuid();
        var totalAmount = 123.45m;
        var status = default(OrderStatus);

        // Act
        var entity = new Order(customerId, totalAmount, status);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.CustomerId.Should().Be(Guid.NewGuid());
        entity.TotalAmount.Should().Be(123.45m);
        entity.Status.Should().Be(default(OrderStatus));
    }

    [Fact]
    public void Constructor_ShouldRaiseOrderCreatedEvent()
    {
        // Arrange
var customerId = Guid.NewGuid();
        var totalAmount = 123.45m;
        var status = default(OrderStatus);

        // Act
        var entity = new Order(customerId, totalAmount, status);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreatedEvent>();
    }


    [Fact]
    public void Create_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new OrderBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.Create();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }
    [Fact]
    public void AddItem_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new OrderBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.AddItem();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }
    [Fact]
    public void RemoveItem_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new OrderBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.RemoveItem();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }
    [Fact]
    public void Confirm_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new OrderBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.Confirm();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }
    [Fact]
    public void Cancel_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new OrderBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.Cancel();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }

    [Theory]
    [AutoData]
    public void Constructor_WithBuilder_ShouldCreateValidEntity(OrderBuilder builder)
    {
        // Act
        var entity = builder.Build();

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.DomainEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var entity = new OrderBuilder().Build();
        entity.DomainEvents.Should().NotBeEmpty();

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }
}