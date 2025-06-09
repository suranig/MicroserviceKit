using ECommerce.OrderService.Application.Order.Commands.UpdateOrder;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;
using ECommerce.OrderService.UnitTests.Utilities;

namespace ECommerce.OrderService.UnitTests.Application.Commands;

public class UpdateOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _mockRepository;
    private readonly Mock<ILogger<UpdateOrderCommandHandler>> _mockLogger;
    private readonly UpdateOrderCommandHandler _handler;
    private readonly Fixture _fixture;

    public UpdateOrderCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Order>>();
        _mockLogger = new Mock<ILogger<UpdateOrderCommandHandler>>();
        _handler = new UpdateOrderCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new OrderBuilder().WithId(entityId).Build();
        var command = new UpdateOrderCommand(entityId, Guid.NewGuid(), 123.45m, "Test Value");

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingEntity);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(existingEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentEntity_ShouldThrowNotFoundException()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var command = new UpdateOrderCommand(entityId, Guid.NewGuid(), 123.45m, "Test Value");

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Order?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}