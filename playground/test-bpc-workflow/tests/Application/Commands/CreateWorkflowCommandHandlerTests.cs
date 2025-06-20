using WorkflowService.Application.Workflow.Commands.CreateWorkflow;
using WorkflowService.Application.Common;
using WorkflowService.Domain.Entities;
using WorkflowService.UnitTests.Builders;
using WorkflowService.UnitTests.Utilities;

namespace WorkflowService.UnitTests.Application.Commands;

public class CreateWorkflowCommandHandlerTests
{
    private readonly Mock<IRepository<Workflow>> _mockRepository;
    private readonly Mock<ILogger<CreateWorkflowCommandHandler>> _mockLogger;
    private readonly CreateWorkflowCommandHandler _handler;
    private readonly Fixture _fixture;

    public CreateWorkflowCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Workflow>>();
        _mockLogger = new Mock<ILogger<CreateWorkflowCommandHandler>>();
        _handler = new CreateWorkflowCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var command = new CreateWorkflowCommand(Guid.NewGuid(), "Test Value", "Test Value");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Workflow>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task Handle_WithAutoDataCommand_ShouldCreateEntity(CreateWorkflowCommand command)
    {
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.Is<Workflow>(e => 
e.Id == command.Id &&
            e.Name == command.Name &&
            e.Description == command.Description
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateWorkflowCommand(Guid.NewGuid(), "Test Value", "Test Value");
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Workflow>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}