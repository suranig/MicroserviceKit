using WorkflowService.Application.Workflow.DTOs;
using WorkflowService.Application.Common;
using MassTransit;

namespace WorkflowService.Application.Workflow.Queries.GetWorkflows;

public class GetWorkflowsQueryHandler : IConsumer<GetWorkflowsQuery>
{
    private readonly IRepository<WorkflowService.Domain.Entities.Workflow> _repository;

    public GetWorkflowsQueryHandler(IRepository<WorkflowService.Domain.Entities.Workflow> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetWorkflowsQuery> context)
    {
        var query = context.Message;
        var entities = await _repository.GetAllAsync(context.CancellationToken);
        var result = entities.Select(MapToDto).ToList();
        await context.RespondAsync(result);
    }

    private WorkflowDto MapToDto(WorkflowService.Domain.Entities.Workflow entity)
    {
        return new WorkflowDto
        {
            Id = entity.Id,
            // TODO: Map other properties
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}