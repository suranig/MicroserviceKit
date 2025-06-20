using TestService.Application.Product.Commands.DeleteProduct;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;
using TestService.UnitTests.Utilities;

namespace TestService.UnitTests.Application.Commands;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _mockRepository;
    private readonly Mock<ILogger<DeleteProductCommandHandler>> _mockLogger;
    private readonly DeleteProductCommandHandler _handler;
    private readonly Fixture _fixture;

    public DeleteProductCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Product>>();
        _mockLogger = new Mock<ILogger<DeleteProductCommandHandler>>();
        _handler = new DeleteProductCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new ProductBuilder().WithId(entityId).Build();
        var command = new DeleteProductCommand(entityId);

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
        var command = new DeleteProductCommand(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Product?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}