using EventStoreService.Application.Event.Commands.DeleteEvent;
using EventStoreService.Application.Common;
using EventStoreService.Domain.Entities;
using EventStoreService.UnitTests.Builders;
using EventStoreService.UnitTests.Utilities;

namespace EventStoreService.UnitTests.Application.Commands;

public class DeleteEventCommandHandlerTests
{
    private readonly Mock<IRepository<Event>> _mockRepository;
    private readonly Mock<ILogger<DeleteEventCommandHandler>> _mockLogger;
    private readonly DeleteEventCommandHandler _handler;
    private readonly Fixture _fixture;

    public DeleteEventCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Event>>();
        _mockLogger = new Mock<ILogger<DeleteEventCommandHandler>>();
        _handler = new DeleteEventCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new EventBuilder().WithId(entityId).Build();
        var command = new DeleteEventCommand(entityId);

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
        var command = new DeleteEventCommand(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Event?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}