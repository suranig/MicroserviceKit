using ReadModelService.Application.Page.Commands.DeletePage;
using ReadModelService.Application.Common;
using ReadModelService.Domain.Entities;
using ReadModelService.UnitTests.Builders;
using ReadModelService.UnitTests.Utilities;

namespace ReadModelService.UnitTests.Application.Commands;

public class DeletePageCommandHandlerTests
{
    private readonly Mock<IRepository<Page>> _mockRepository;
    private readonly Mock<ILogger<DeletePageCommandHandler>> _mockLogger;
    private readonly DeletePageCommandHandler _handler;
    private readonly Fixture _fixture;

    public DeletePageCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Page>>();
        _mockLogger = new Mock<ILogger<DeletePageCommandHandler>>();
        _handler = new DeletePageCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new PageBuilder().WithId(entityId).Build();
        var command = new DeletePageCommand(entityId);

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
        var command = new DeletePageCommand(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Page?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}