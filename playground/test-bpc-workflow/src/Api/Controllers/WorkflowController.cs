using Microsoft.AspNetCore.Mvc;
using WorkflowService.Application.Workflow.Commands.CreateWorkflow;
using WorkflowService.Application.Workflow.Commands.UpdateWorkflow;
using WorkflowService.Application.Workflow.Commands.DeleteWorkflow;
using WorkflowService.Application.Workflow.Queries.GetWorkflowById;
using WorkflowService.Application.Workflow.Queries.GetWorkflows;
using WorkflowService.Application.Workflow.Queries.GetWorkflowsWithPaging;
using WorkflowService.Application.Workflow.DTOs;
using WorkflowService.Api.Models;
using Wolverine;

namespace WorkflowService.Api.Controllers;

[ApiController]
[Route("api/rest/workflow")]
[Produces("application/json")]
[Tags("Workflow Management")]
public class WorkflowController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(IMessageBus messageBus, ILogger<WorkflowController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Get all workflows with paging
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged list of workflows</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<WorkflowResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<WorkflowResponse>>> GetWorkflows(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting workflows with page={Page}, pageSize={PageSize}", page, pageSize);
        
        var query = new GetWorkflowsWithPagingQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<PagedResult<WorkflowDto>>(query, cancellationToken);
        
        var response = new PagedResponse<WorkflowResponse>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
        
        return Ok(response);
    }

    /// <summary>
    /// Get workflow by ID
    /// </summary>
    /// <param name="id">Workflow ID</param>
    /// <returns>Workflow details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkflowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkflowResponse>> GetWorkflowById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting workflow with id={Id}", id);
        
        var query = new GetWorkflowByIdQuery(id);
        var result = await _messageBus.InvokeAsync<WorkflowDto?>(query, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("Workflow with id={Id} not found", id);
            return NotFound();
        }
        
        return Ok(MapToResponse(result));
    }

    /// <summary>
    /// Create new workflow
    /// </summary>
    /// <param name="request">Create workflow request</param>
    /// <returns>Created workflow ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateWorkflow(
        [FromBody] CreateWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new workflow");
        
        var command = MapToCreateCommand(request);
        var id = await _messageBus.InvokeAsync<Guid>(command, cancellationToken);
        
        _logger.LogInformation("Workflow created with id={Id}", id);
        return CreatedAtAction(nameof(GetWorkflowById), new { id }, id);
    }

    /// <summary>
    /// Update existing workflow
    /// </summary>
    /// <param name="id">Workflow ID</param>
    /// <param name="request">Update workflow request</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWorkflow(
        Guid id,
        [FromBody] UpdateWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating workflow with id={Id}", id);
        
        var command = MapToUpdateCommand(id, request);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Workflow with id={Id} updated successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Delete workflow
    /// </summary>
    /// <param name="id">Workflow ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkflow(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting workflow with id={Id}", id);
        
        var command = new DeleteWorkflowCommand(id);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Workflow with id={Id} deleted successfully", id);
        return NoContent();
    }

    // Mapping methods
    private static WorkflowResponse MapToResponse(WorkflowDto dto)
    {
        return new WorkflowResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description
        };
    }

    private static CreateWorkflowCommand MapToCreateCommand(CreateWorkflowRequest request)
    {
        return new CreateWorkflowCommand(
            request.Id,
            request.Name,
            request.Description);
    }

    private static UpdateWorkflowCommand MapToUpdateCommand(Guid id, UpdateWorkflowRequest request)
    {
        return new UpdateWorkflowCommand(
            id,
            request.Id,
            request.Name,
            request.Description);
    }
}