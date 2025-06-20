using WorkflowService.Application.Workflow.DTOs;
using WorkflowService.Application.Common;
using MassTransit;

namespace WorkflowService.Application.Workflow.Queries.GetWorkflowById;

public class GetWorkflowByIdQueryHandler : IConsumer<GetWorkflowByIdQuery>
{
    private readonly IRepository<WorkflowService.Domain.Entities.Workflow> _repository;

    public GetWorkflowByIdQueryHandler(IRepository<WorkflowService.Domain.Entities.Workflow> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetWorkflowByIdQuery> context)
    {
        var query = context.Message;
        var entity = await _repository.GetByIdAsync(query.Id, context.CancellationToken);
        var result = entity == null ? null : MapToDto(entity);
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