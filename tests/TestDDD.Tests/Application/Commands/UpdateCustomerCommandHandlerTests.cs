using ECommerce.OrderService.Application.Customer.Commands.UpdateCustomer;
using ECommerce.OrderService.Application.Common;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.UnitTests.Builders;
using ECommerce.OrderService.UnitTests.Utilities;

namespace ECommerce.OrderService.UnitTests.Application.Commands;

public class UpdateCustomerCommandHandlerTests
{
    private readonly Mock<IRepository<Customer>> _mockRepository;
    private readonly Mock<ILogger<UpdateCustomerCommandHandler>> _mockLogger;
    private readonly UpdateCustomerCommandHandler _handler;
    private readonly Fixture _fixture;

    public UpdateCustomerCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Customer>>();
        _mockLogger = new Mock<ILogger<UpdateCustomerCommandHandler>>();
        _handler = new UpdateCustomerCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new CustomerBuilder().WithId(entityId).Build();
        var command = new UpdateCustomerCommand(entityId, "Test Value", "Test Value");

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
        var command = new UpdateCustomerCommand(entityId, "Test Value", "Test Value");

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Customer?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}