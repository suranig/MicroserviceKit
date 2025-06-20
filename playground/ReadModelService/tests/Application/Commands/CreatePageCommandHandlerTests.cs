using ReadModelService.Application.Page.Commands.CreatePage;
using ReadModelService.Application.Common;
using ReadModelService.Domain.Entities;
using ReadModelService.UnitTests.Builders;
using ReadModelService.UnitTests.Utilities;

namespace ReadModelService.UnitTests.Application.Commands;

public class CreatePageCommandHandlerTests
{
    private readonly Mock<IRepository<Page>> _mockRepository;
    private readonly Mock<ILogger<CreatePageCommandHandler>> _mockLogger;
    private readonly CreatePageCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreatePageCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Page>>();
        _mockLogger = new Mock<ILogger<CreatePageCommandHandler>>();
        _handler = new CreatePageCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreatePageCommand(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Page>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task Handle_WithAutoDataCommand_ShouldCreateEntity(CreatePageCommand command)
    {
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.Is<Page>(e => 
e.Id == command.Id &&
            e.CreatedAt == command.CreatedAt &&
            e.UpdatedAt == command.UpdatedAt
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreatePageCommand(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Page>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}