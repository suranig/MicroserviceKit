using MyApp.UserService.Domain.Entities;
using MyApp.UserService.Domain.Events;
using MyApp.UserService.UnitTests.Builders;

namespace MyApp.UserService.UnitTests.Domain.Entities;

public class UserTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEntity()
    {
        // Arrange
var email = "Test Value";
        var name = "Test Value";

        // Act
        var entity = new User(email, name);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.Email.Should().Be("Test Value");
        entity.Name.Should().Be("Test Value");
    }

    [Fact]
    public void Constructor_ShouldRaiseUserCreatedEvent()
    {
        // Arrange
var email = "Test Value";
        var name = "Test Value";

        // Act
        var entity = new User(email, name);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedEvent>();
    }


    [Fact]
    public void Create_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new UserBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.Create();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }
    [Fact]
    public void UpdateEmail_ShouldExecuteSuccessfully()
    {
        // Arrange
        var entity = new UserBuilder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.UpdateEmail();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }

    [Theory]
    [AutoData]
    public void Constructor_WithBuilder_ShouldCreateValidEntity(UserBuilder builder)
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
        var entity = new UserBuilder().Build();
        entity.DomainEvents.Should().NotBeEmpty();

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }
}