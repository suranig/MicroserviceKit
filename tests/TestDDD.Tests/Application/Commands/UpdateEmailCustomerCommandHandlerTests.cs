using ECommerce.OrderService.Application.Customer.Commands.UpdateEmailCustomer;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;
using ECommerce.OrderService.UnitTests.Utilities;

namespace ECommerce.OrderService.UnitTests.Application.Commands;

public class UpdateEmailCustomerCommandHandlerTests
{
    private readonly Mock<IRepository<Customer>> _mockRepository;
    private readonly Mock<ILogger<UpdateEmailCustomerCommandHandler>> _mockLogger;
    private readonly UpdateEmailCustomerCommandHandler _handler;
    private readonly Fixture _fixture;

    public UpdateEmailCustomerCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Customer>>();
        _mockLogger = new Mock<ILogger<UpdateEmailCustomerCommandHandler>>();
        _handler = new UpdateEmailCustomerCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldExecuteUpdateEmailSuccessfully()
    {
        // Arrange
        var command = new UpdateEmailCustomerCommand("Test Value", "Test Value");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Add specific assertions for UpdateEmail operation
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}