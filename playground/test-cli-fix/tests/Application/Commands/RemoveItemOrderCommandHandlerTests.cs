using TestService.Application.Order.Commands.RemoveItemOrder;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;
using TestService.UnitTests.Utilities;

namespace TestService.UnitTests.Application.Commands;

public class RemoveItemOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _mockRepository;
    private readonly Mock<ILogger<RemoveItemOrderCommandHandler>> _mockLogger;
    private readonly RemoveItemOrderCommandHandler _handler;
    private readonly Fixture _fixture;

    public RemoveItemOrderCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Order>>();
        _mockLogger = new Mock<ILogger<RemoveItemOrderCommandHandler>>();
        _handler = new RemoveItemOrderCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldExecuteRemoveItemSuccessfully()
    {
        // Arrange
        var command = new RemoveItemOrderCommand(Guid.NewGuid(), 123.45m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Add specific assertions for RemoveItem operation
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}