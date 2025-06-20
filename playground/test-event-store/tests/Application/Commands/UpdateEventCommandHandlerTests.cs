using EventStoreService.Application.Event.Commands.UpdateEvent;
using EventStoreService.Application.Common;
using EventStoreService.Domain.Entities;
using EventStoreService.UnitTests.Builders;
using EventStoreService.UnitTests.Utilities;

namespace EventStoreService.UnitTests.Application.Commands;

public class UpdateEventCommandHandlerTests
{
    private readonly Mock<IRepository<Event>> _mockRepository;
    private readonly Mock<ILogger<UpdateEventCommandHandler>> _mockLogger;
    private readonly UpdateEventCommandHandler _handler;
    private readonly Fixture _fixture;

    public UpdateEventCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Event>>();
        _mockLogger = new Mock<ILogger<UpdateEventCommandHandler>>();
        _handler = new UpdateEventCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new EventBuilder().WithId(entityId).Build();
        var command = new UpdateEventCommand(entityId, Guid.NewGuid(), "Test Value", "Test Value");

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
        var command = new UpdateEventCommand(entityId, Guid.NewGuid(), "Test Value", "Test Value");

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Event?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}