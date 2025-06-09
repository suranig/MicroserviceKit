using ECommerce.OrderService.Application.Order.Commands.ConfirmOrder;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;
using ECommerce.OrderService.UnitTests.Utilities;

namespace ECommerce.OrderService.UnitTests.Application.Commands;

public class ConfirmOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _mockRepository;
    private readonly Mock<ILogger<ConfirmOrderCommandHandler>> _mockLogger;
    private readonly ConfirmOrderCommandHandler _handler;
    private readonly Fixture _fixture;

    public ConfirmOrderCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Order>>();
        _mockLogger = new Mock<ILogger<ConfirmOrderCommandHandler>>();
        _handler = new ConfirmOrderCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldExecuteConfirmSuccessfully()
    {
        // Arrange
        var command = new ConfirmOrderCommand(Guid.NewGuid(), 123.45m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Add specific assertions for Confirm operation
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}