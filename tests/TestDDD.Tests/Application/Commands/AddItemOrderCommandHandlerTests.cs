using ECommerce.OrderService.Application.Order.Commands.AddItemOrder;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;
using ECommerce.OrderService.UnitTests.Utilities;

namespace ECommerce.OrderService.UnitTests.Application.Commands;

public class AddItemOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _mockRepository;
    private readonly Mock<ILogger<AddItemOrderCommandHandler>> _mockLogger;
    private readonly AddItemOrderCommandHandler _handler;
    private readonly Fixture _fixture;

    public AddItemOrderCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Order>>();
        _mockLogger = new Mock<ILogger<AddItemOrderCommandHandler>>();
        _handler = new AddItemOrderCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldExecuteAddItemSuccessfully()
    {
        // Arrange
        var command = new AddItemOrderCommand(Guid.NewGuid(), 123.45m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Add specific assertions for AddItem operation
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}