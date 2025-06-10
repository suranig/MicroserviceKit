using Company.TestService.Application.Item.Commands.UpdateItem;
using Company.TestService.Application.Common;
using Company.TestService.Domain.Entities;
using Company.TestService.UnitTests.Builders;
using Company.TestService.UnitTests.Utilities;

namespace Company.TestService.UnitTests.Application.Commands;

public class UpdateItemCommandHandlerTests
{
    private readonly Mock<IRepository<Item>> _mockRepository;
    private readonly Mock<ILogger<UpdateItemCommandHandler>> _mockLogger;
    private readonly UpdateItemCommandHandler _handler;
    private readonly Fixture _fixture;

    public UpdateItemCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Item>>();
        _mockLogger = new Mock<ILogger<UpdateItemCommandHandler>>();
        _handler = new UpdateItemCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new ItemBuilder().WithId(entityId).Build();
        var command = new UpdateItemCommand(entityId, "Test Value", true);

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
        var command = new UpdateItemCommand(entityId, "Test Value", true);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Item?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}