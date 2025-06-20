using WorkflowService.Application.Workflow.DTOs;
using WorkflowService.Application.Common;
using MassTransit;

namespace WorkflowService.Application.Workflow.Queries.GetWorkflowsWithPaging;

public class GetWorkflowsWithPagingQueryHandler : IConsumer<GetWorkflowsWithPagingQuery>
{
    private readonly IRepository<WorkflowService.Domain.Entities.Workflow> _repository;

    public GetWorkflowsWithPagingQueryHandler(IRepository<WorkflowService.Domain.Entities.Workflow> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetWorkflowsWithPagingQuery> context)
    {
        var query = context.Message;
        var pagedResult = await _repository.GetPagedAsync(query.Page, query.PageSize, context.CancellationToken);
        var result = new PagedResult<WorkflowDto>
        {
            Items = pagedResult.Items.Select(MapToDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
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