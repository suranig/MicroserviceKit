using Microsoft.AspNetCore.Mvc;
using ReadModelService.Application.Page.Commands.CreatePage;
using ReadModelService.Application.Page.Commands.UpdatePage;
using ReadModelService.Application.Page.Commands.DeletePage;
using ReadModelService.Application.Page.Queries.GetPageById;
using ReadModelService.Application.Page.Queries.GetPages;
using ReadModelService.Application.Page.Queries.GetPagesWithPaging;
using ReadModelService.Application.Page.DTOs;
using ReadModelService.Api.Models;
using Wolverine;

namespace ReadModelService.Api.Controllers;

[ApiController]
[Route("api/rest/page")]
[Produces("application/json")]
[Tags("Page Management")]
public class PageController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<PageController> _logger;

    public PageController(IMessageBus messageBus, ILogger<PageController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Get all pages with paging
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged list of pages</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PageResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<PageResponse>>> GetPages(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting pages with page={Page}, pageSize={PageSize}", page, pageSize);
        
        var query = new GetPagesWithPagingQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<PagedResult<PageDto>>(query, cancellationToken);
        
        var response = new PagedResponse<PageResponse>
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
    /// Get page by ID
    /// </summary>
    /// <param name="id">Page ID</param>
    /// <returns>Page details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PageResponse>> GetPageById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting page with id={Id}", id);
        
        var query = new GetPageByIdQuery(id);
        var result = await _messageBus.InvokeAsync<PageDto?>(query, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("Page with id={Id} not found", id);
            return NotFound();
        }
        
        return Ok(MapToResponse(result));
    }

    /// <summary>
    /// Create new page
    /// </summary>
    /// <param name="request">Create page request</param>
    /// <returns>Created page ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreatePage(
        [FromBody] CreatePageRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new page");
        
        var command = MapToCreateCommand(request);
        var id = await _messageBus.InvokeAsync<Guid>(command, cancellationToken);
        
        _logger.LogInformation("Page created with id={Id}", id);
        return CreatedAtAction(nameof(GetPageById), new { id }, id);
    }

    /// <summary>
    /// Update existing page
    /// </summary>
    /// <param name="id">Page ID</param>
    /// <param name="request">Update page request</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePage(
        Guid id,
        [FromBody] UpdatePageRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating page with id={Id}", id);
        
        var command = MapToUpdateCommand(id, request);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Page with id={Id} updated successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Delete page
    /// </summary>
    /// <param name="id">Page ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting page with id={Id}", id);
        
        var command = new DeletePageCommand(id);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Page with id={Id} deleted successfully", id);
        return NoContent();
    }

    // Mapping methods
    private static PageResponse MapToResponse(PageDto dto)
    {
        return new PageResponse
        {
            Id = dto.Id,
            Id = dto.Id,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    private static CreatePageCommand MapToCreateCommand(CreatePageRequest request)
    {
        return new CreatePageCommand(
            request.Id,
            request.CreatedAt,
            request.UpdatedAt);
    }

    private static UpdatePageCommand MapToUpdateCommand(Guid id, UpdatePageRequest request)
    {
        return new UpdatePageCommand(
            id,
            request.Id,
            request.CreatedAt,
            request.UpdatedAt);
    }
}