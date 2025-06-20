using EventStoreService.Application.Event.Commands.CreateEvent;
using EventStoreService.Application.Common;
using EventStoreService.Domain.Entities;
using EventStoreService.UnitTests.Builders;
using EventStoreService.UnitTests.Utilities;

namespace EventStoreService.UnitTests.Application.Commands;

public class CreateEventCommandHandlerTests
{
    private readonly Mock<IRepository<Event>> _mockRepository;
    private readonly Mock<ILogger<CreateEventCommandHandler>> _mockLogger;
    private readonly CreateEventCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateEventCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Event>>();
        _mockLogger = new Mock<ILogger<CreateEventCommandHandler>>();
        _handler = new CreateEventCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateEventCommand(Guid.NewGuid(), "Test Value", "Test Value");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task Handle_WithAutoDataCommand_ShouldCreateEntity(CreateEventCommand command)
    {
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.Is<Event>(e => 
e.Id == command.Id &&
            e.Name == command.Name &&
            e.Description == command.Description
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateEventCommand(Guid.NewGuid(), "Test Value", "Test Value");
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}