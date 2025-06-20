using ReadModelService.Application.Page.Commands.UpdatePage;
using ReadModelService.Application.Common;
using ReadModelService.Domain.Entities;
using ReadModelService.UnitTests.Builders;
using ReadModelService.UnitTests.Utilities;

namespace ReadModelService.UnitTests.Application.Commands;

public class UpdatePageCommandHandlerTests
{
    private readonly Mock<IRepository<Page>> _mockRepository;
    private readonly Mock<ILogger<UpdatePageCommandHandler>> _mockLogger;
    private readonly UpdatePageCommandHandler _handler;
    private readonly Fixture _fixture;

    public UpdatePageCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Page>>();
        _mockLogger = new Mock<ILogger<UpdatePageCommandHandler>>();
        _handler = new UpdatePageCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new PageBuilder().WithId(entityId).Build();
        var command = new UpdatePageCommand(entityId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow);

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
        var command = new UpdatePageCommand(entityId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Page?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}