using Microsoft.AspNetCore.Mvc;
using Company.TestService.Application.Item.Commands.CreateItem;
using Company.TestService.Application.Item.Commands.UpdateItem;
using Company.TestService.Application.Item.Commands.DeleteItem;
using Company.TestService.Application.Item.Queries.GetItemById;
using Company.TestService.Application.Item.Queries.GetItems;
using Company.TestService.Application.Item.Queries.GetItemsWithPaging;
using Company.TestService.Application.Item.DTOs;
using Company.TestService.Api.Models;
using Wolverine;

namespace Company.TestService.Api.Controllers;

[ApiController]
[Route("api/rest/item")]
[Produces("application/json")]
[Tags("Item Management")]
public class ItemController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<ItemController> _logger;

    public ItemController(IMessageBus messageBus, ILogger<ItemController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Get all items with paging
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged list of items</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ItemResponse>>> GetItems(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting items with page={Page}, pageSize={PageSize}", page, pageSize);
        
        var query = new GetItemsWithPagingQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<PagedResult<ItemDto>>(query, cancellationToken);
        
        var response = new PagedResponse<ItemResponse>
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
    /// Get item by ID
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>Item details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemResponse>> GetItemById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting item with id={Id}", id);
        
        var query = new GetItemByIdQuery(id);
        var result = await _messageBus.InvokeAsync<ItemDto?>(query, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("Item with id={Id} not found", id);
            return NotFound();
        }
        
        return Ok(MapToResponse(result));
    }

    /// <summary>
    /// Create new item
    /// </summary>
    /// <param name="request">Create item request</param>
    /// <returns>Created item ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateItem(
        [FromBody] CreateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new item");
        
        var command = MapToCreateCommand(request);
        var id = await _messageBus.InvokeAsync<Guid>(command, cancellationToken);
        
        _logger.LogInformation("Item created with id={Id}", id);
        return CreatedAtAction(nameof(GetItemById), new { id }, id);
    }

    /// <summary>
    /// Update existing item
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="request">Update item request</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(
        Guid id,
        [FromBody] UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating item with id={Id}", id);
        
        var command = MapToUpdateCommand(id, request);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Item with id={Id} updated successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Delete item
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting item with id={Id}", id);
        
        var command = new DeleteItemCommand(id);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Item with id={Id} deleted successfully", id);
        return NoContent();
    }

    // Mapping methods
    private static ItemResponse MapToResponse(ItemDto dto)
    {
        return new ItemResponse
        {
            Id = dto.Id,
            Title = dto.Title,
            IsCompleted = dto.IsCompleted
        };
    }

    private static CreateItemCommand MapToCreateCommand(CreateItemRequest request)
    {
        return new CreateItemCommand(
            request.Title,
            request.IsCompleted);
    }

    private static UpdateItemCommand MapToUpdateCommand(Guid id, UpdateItemRequest request)
    {
        return new UpdateItemCommand(
            id,
            request.Title,
            request.IsCompleted);
    }
}