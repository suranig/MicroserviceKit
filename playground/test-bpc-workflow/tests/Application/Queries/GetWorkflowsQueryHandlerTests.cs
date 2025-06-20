using WorkflowService.Application.Workflow.Queries.GetWorkflows;
using WorkflowService.Application.Workflow.DTOs;
using WorkflowService.Application.Common;
using WorkflowService.Domain.Entities;
using WorkflowService.UnitTests.Builders;

namespace WorkflowService.UnitTests.Application.Queries;

public class GetWorkflowsQueryHandlerTests
{
    private readonly Mock<IRepository<Workflow>> _mockRepository;
    private readonly GetWorkflowsQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetWorkflowsQueryHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Workflow>>();
        _handler = new GetWorkflowsQueryHandler(_mockRepository.Object);
        _fixture = new Fixture();
    }


    [Fact]
    public async Task Handle_WithExistingEntities_ShouldReturnDtos()
    {
        // Arrange
        var entities = new List<Workflow>
        {
            new WorkflowBuilder().Build(),
            new WorkflowBuilder().Build(),
            new WorkflowBuilder().Build()
        };
        var query = new GetWorkflowsQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(dto => dto.Id.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Handle_WithNoEntities_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetWorkflowsQuery();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Workflow>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}