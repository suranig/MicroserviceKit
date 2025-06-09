using ECommerce.OrderService.Application.Customer.Commands.UpdateNameCustomer;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;
using ECommerce.OrderService.UnitTests.Utilities;

namespace ECommerce.OrderService.UnitTests.Application.Commands;

public class UpdateNameCustomerCommandHandlerTests
{
    private readonly Mock<IRepository<Customer>> _mockRepository;
    private readonly Mock<ILogger<UpdateNameCustomerCommandHandler>> _mockLogger;
    private readonly UpdateNameCustomerCommandHandler _handler;
    private readonly Fixture _fixture;

    public UpdateNameCustomerCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Customer>>();
        _mockLogger = new Mock<ILogger<UpdateNameCustomerCommandHandler>>();
        _handler = new UpdateNameCustomerCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldExecuteUpdateNameSuccessfully()
    {
        // Arrange
        var command = new UpdateNameCustomerCommand("Test Value", "Test Value");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Add specific assertions for UpdateName operation
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}