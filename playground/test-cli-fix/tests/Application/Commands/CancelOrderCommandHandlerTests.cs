using TestService.Application.Order.Commands.CancelOrder;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;
using TestService.UnitTests.Utilities;

namespace TestService.UnitTests.Application.Commands;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _mockRepository;
    private readonly Mock<ILogger<CancelOrderCommandHandler>> _mockLogger;
    private readonly CancelOrderCommandHandler _handler;
    private readonly Fixture _fixture;

    public CancelOrderCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Order>>();
        _mockLogger = new Mock<ILogger<CancelOrderCommandHandler>>();
        _handler = new CancelOrderCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldExecuteCancelSuccessfully()
    {
        // Arrange
        var command = new CancelOrderCommand(Guid.NewGuid(), 123.45m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Add specific assertions for Cancel operation
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}