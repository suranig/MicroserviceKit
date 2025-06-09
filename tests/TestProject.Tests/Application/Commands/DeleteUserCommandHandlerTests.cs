using MyApp.UserService.Application.User.Commands.DeleteUser;
using MyApp.UserService.Application.Common;
using MyApp.UserService.Domain.Entities;
using MyApp.UserService.UnitTests.Builders;
using MyApp.UserService.UnitTests.Utilities;

namespace MyApp.UserService.UnitTests.Application.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _mockRepository;
    private readonly Mock<ILogger<DeleteUserCommandHandler>> _mockLogger;
    private readonly DeleteUserCommandHandler _handler;
    private readonly Fixture _fixture;

    public DeleteUserCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<User>>();
        _mockLogger = new Mock<ILogger<DeleteUserCommandHandler>>();
        _handler = new DeleteUserCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new UserBuilder().WithId(entityId).Build();
        var command = new DeleteUserCommand(entityId);

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
        var command = new DeleteUserCommand(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((User?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}