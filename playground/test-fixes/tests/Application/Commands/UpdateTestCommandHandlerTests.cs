using TestService.Application.Test.Commands.UpdateTest;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;
using TestService.UnitTests.Utilities;

namespace TestService.UnitTests.Application.Commands;

public class UpdateTestCommandHandlerTests
{
    private readonly Mock<IRepository<Test>> _mockRepository;
    private readonly Mock<ILogger<UpdateTestCommandHandler>> _mockLogger;
    private readonly UpdateTestCommandHandler _handler;
    private readonly Fixture _fixture;

    public UpdateTestCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Test>>();
        _mockLogger = new Mock<ILogger<UpdateTestCommandHandler>>();
        _handler = new UpdateTestCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new TestBuilder().WithId(entityId).Build();
        var command = new UpdateTestCommand(entityId, Guid.NewGuid(), "Test Value", "Test Value", DateTime.UtcNow, DateTime.UtcNow);

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
        var command = new UpdateTestCommand(entityId, Guid.NewGuid(), "Test Value", "Test Value", DateTime.UtcNow, DateTime.UtcNow);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Test?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}