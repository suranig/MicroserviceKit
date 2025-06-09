using MyApp.UserService.Application.User.Commands.CreateUser;
using MyApp.UserService.Application.Common;
using MyApp.UserService.Domain.Entities;
using MyApp.UserService.UnitTests.Builders;
using MyApp.UserService.UnitTests.Utilities;

namespace MyApp.UserService.UnitTests.Application.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _mockRepository;
    private readonly Mock<ILogger<CreateUserCommandHandler>> _mockLogger;
    private readonly CreateUserCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateUserCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<User>>();
        _mockLogger = new Mock<ILogger<CreateUserCommandHandler>>();
        _handler = new CreateUserCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateUserCommand("Test Value", "Test Value");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task Handle_WithAutoDataCommand_ShouldCreateEntity(CreateUserCommand command)
    {
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.Is<User>(e => 
e.Email == command.Email &&
            e.Name == command.Name
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateUserCommand("Test Value", "Test Value");
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}