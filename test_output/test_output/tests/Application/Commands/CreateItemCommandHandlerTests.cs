using Company.TestService.Application.Item.Commands.CreateItem;
using Company.TestService.Application.Common;
using Company.TestService.Domain.Entities;
using Company.TestService.UnitTests.Builders;
using Company.TestService.UnitTests.Utilities;

namespace Company.TestService.UnitTests.Application.Commands;

public class CreateItemCommandHandlerTests
{
    private readonly Mock<IRepository<Item>> _mockRepository;
    private readonly Mock<ILogger<CreateItemCommandHandler>> _mockLogger;
    private readonly CreateItemCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateItemCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Item>>();
        _mockLogger = new Mock<ILogger<CreateItemCommandHandler>>();
        _handler = new CreateItemCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateItemCommand("Test Value", true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task Handle_WithAutoDataCommand_ShouldCreateEntity(CreateItemCommand command)
    {
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.Is<Item>(e => 
e.Title == command.Title &&
            e.IsCompleted == command.IsCompleted
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateItemCommand("Test Value", true);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}