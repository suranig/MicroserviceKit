using TestService.Application.Test.Commands.DeleteTest;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;
using TestService.UnitTests.Utilities;

namespace TestService.UnitTests.Application.Commands;

public class DeleteTestCommandHandlerTests
{
    private readonly Mock<IRepository<Test>> _mockRepository;
    private readonly Mock<ILogger<DeleteTestCommandHandler>> _mockLogger;
    private readonly DeleteTestCommandHandler _handler;
    private readonly Fixture _fixture;

    public DeleteTestCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Test>>();
        _mockLogger = new Mock<ILogger<DeleteTestCommandHandler>>();
        _handler = new DeleteTestCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new TestBuilder().WithId(entityId).Build();
        var command = new DeleteTestCommand(entityId);

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
        var command = new DeleteTestCommand(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Test?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}