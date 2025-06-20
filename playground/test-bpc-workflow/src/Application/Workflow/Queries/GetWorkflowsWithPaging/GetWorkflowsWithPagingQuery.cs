using WorkflowService.Application.Workflow.DTOs;

namespace WorkflowService.Application.Workflow.Queries.GetWorkflowsWithPaging;

public record GetWorkflowsWithPagingQuery(int Page = 1, int PageSize = 10);