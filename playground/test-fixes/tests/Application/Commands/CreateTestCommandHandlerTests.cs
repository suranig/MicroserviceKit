using TestService.Application.Test.Commands.CreateTest;
using TestService.Application.Common;
using TestService.Domain.Entities;
using TestService.UnitTests.Builders;
using TestService.UnitTests.Utilities;

namespace TestService.UnitTests.Application.Commands;

public class CreateTestCommandHandlerTests
{
    private readonly Mock<IRepository<Test>> _mockRepository;
    private readonly Mock<ILogger<CreateTestCommandHandler>> _mockLogger;
    private readonly CreateTestCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateTestCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Test>>();
        _mockLogger = new Mock<ILogger<CreateTestCommandHandler>>();
        _handler = new CreateTestCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateTestCommand(Guid.NewGuid(), "Test Value", "Test Value", DateTime.UtcNow, DateTime.UtcNow);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task Handle_WithAutoDataCommand_ShouldCreateEntity(CreateTestCommand command)
    {
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.Is<Test>(e => 
e.Id == command.Id &&
            e.Name == command.Name &&
            e.Description == command.Description &&
            e.CreatedAt == command.CreatedAt &&
            e.UpdatedAt == command.UpdatedAt
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateTestCommand(Guid.NewGuid(), "Test Value", "Test Value", DateTime.UtcNow, DateTime.UtcNow);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Test>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}