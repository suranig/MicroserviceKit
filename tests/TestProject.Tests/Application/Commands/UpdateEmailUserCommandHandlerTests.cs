using MyApp.UserService.Application.User.Commands.UpdateEmailUser;
using MyApp.UserService.Application.Common;
using MyApp.UserService.Domain.Entities;
using MyApp.UserService.UnitTests.Builders;
using MyApp.UserService.UnitTests.Utilities;

namespace MyApp.UserService.UnitTests.Application.Commands;

public class UpdateEmailUserCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _mockRepository;
    private readonly Mock<ILogger<UpdateEmailUserCommandHandler>> _mockLogger;
    private readonly UpdateEmailUserCommandHandler _handler;
    private readonly Fixture _fixture;

    public UpdateEmailUserCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<User>>();
        _mockLogger = new Mock<ILogger<UpdateEmailUserCommandHandler>>();
        _handler = new UpdateEmailUserCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldExecuteUpdateEmailSuccessfully()
    {
        // Arrange
        var command = new UpdateEmailUserCommand("Test Value", "Test Value");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Add specific assertions for UpdateEmail operation
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}