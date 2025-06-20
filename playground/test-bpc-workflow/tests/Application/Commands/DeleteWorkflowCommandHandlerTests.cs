using WorkflowService.Application.Workflow.Commands.DeleteWorkflow;
using WorkflowService.Application.Common;
using WorkflowService.Domain.Entities;
using WorkflowService.UnitTests.Builders;
using WorkflowService.UnitTests.Utilities;

namespace WorkflowService.UnitTests.Application.Commands;

public class DeleteWorkflowCommandHandlerTests
{
    private readonly Mock<IRepository<Workflow>> _mockRepository;
    private readonly Mock<ILogger<DeleteWorkflowCommandHandler>> _mockLogger;
    private readonly DeleteWorkflowCommandHandler _handler;
    private readonly Fixture _fixture;

    public DeleteWorkflowCommandHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Workflow>>();
        _mockLogger = new Mock<ILogger<DeleteWorkflowCommandHandler>>();
        _handler = new DeleteWorkflowCommandHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new WorkflowBuilder().WithId(entityId).Build();
        var command = new DeleteWorkflowCommand(entityId);

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
        var command = new DeleteWorkflowCommand(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Workflow?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}