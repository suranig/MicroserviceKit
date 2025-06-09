using ECommerce.OrderService.Application.Customer.Commands.CreateCustomer;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;
using ECommerce.OrderService.UnitTests.Utilities;

namespace ECommerce.OrderService.UnitTests.Application.Commands;

public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<IRepository<Customer>> _mockRepository;
    private readonly Mock<ILogger<CreateCustomerCommandHandler>> _mockLogger;
    private readonly CreateCustomerCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateCustomerCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Customer>>();
        _mockLogger = new Mock<ILogger<CreateCustomerCommandHandler>>();
        _handler = new CreateCustomerCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateCustomerCommand("Test Value", "Test Value");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task Handle_WithAutoDataCommand_ShouldCreateEntity(CreateCustomerCommand command)
    {
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.Is<Customer>(e => 
e.Email == command.Email &&
            e.Name == command.Name
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateCustomerCommand("Test Value", "Test Value");
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}