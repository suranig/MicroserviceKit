using WorkflowService.Application.Workflow.Queries.GetWorkflowById;
using WorkflowService.Application.Workflow.DTOs;
using WorkflowService.Application.Common;
using WorkflowService.Domain.Entities;
using WorkflowService.UnitTests.Builders;

namespace WorkflowService.UnitTests.Application.Queries;

public class GetWorkflowByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Workflow>> _mockRepository;
    private readonly GetWorkflowByIdQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetWorkflowByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Workflow>>();
        _handler = new GetWorkflowByIdQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntity_ShouldReturnDto()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new WorkflowBuilder().WithId(entityId).Build();
        var query = new GetWorkflowByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entity);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entityId);
    }

    [Fact]
    public async Task Handle_WithNonExistentEntity_ShouldReturnNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var query = new GetWorkflowByIdQuery(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(((Workflow?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}