using OrderService.Application.Order.Commands.DeleteOrder;
using OrderService.Application.Common;
using OrderService.Domain.Entities;
using OrderService.UnitTests.Builders;
using OrderService.UnitTests.Utilities;

namespace OrderService.UnitTests.Application.Commands;

public class DeleteOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _mockRepository;
    private readonly Mock<ILogger<DeleteOrderCommandHandler>> _mockLogger;
    private readonly DeleteOrderCommandHandler _handler;
    private readonly Fixture _fixture;

    public DeleteOrderCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Order>>();
        _mockLogger = new Mock<ILogger<DeleteOrderCommandHandler>>();
        _handler = new DeleteOrderCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new OrderBuilder().WithId(entityId).Build();
        var command = new DeleteOrderCommand(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingEntity);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(existingEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentEntity_ShouldThrowNotFoundException()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var command = new DeleteOrderCommand(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Order?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}