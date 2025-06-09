using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Domain.Events;
using ECommerce.OrderService.UnitTests.Builders;

namespace ECommerce.OrderService.UnitTests.Domain.Entities;

public class CustomerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEntity()
    {
        // Arrange
var email = "Test Value";
        var name = "Test Value";

        // Act
        var entity = new Customer(email, name);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.Email.Should().Be("Test Value");
        entity.Name.Should().Be("Test Value");
    }

    [Fact]
    public void Constructor_ShouldRaiseCustomerCreatedEvent()
    {
        // Arrange
var email = "Test Value";
        var name = "Test Value";

        // Act
        var entity = new Customer(email, name);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CustomerCreatedEvent>();
    }


    [Fact]
    public void UpdateEmail_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new CustomerBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.UpdateEmail();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }
    [Fact]
    public void UpdateName_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new CustomerBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.UpdateName();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }

    [Theory]
    [AutoData]
    public void Constructor_WithBuilder_ShouldCreateValidEntity(CustomerBuilder builder)
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
        var entity = new CustomerBuilder().Build();
        entity.DomainEvents.Should().NotBeEmpty();

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }
}