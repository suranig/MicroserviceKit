using Microsoft.AspNetCore.Mvc;
using TestService.Application.Order.Commands.CreateOrder;
using TestService.Application.Order.Commands.UpdateOrder;
using TestService.Application.Order.Commands.DeleteOrder;
using TestService.Application.Order.Queries.GetOrderById;
using TestService.Application.Order.Queries.GetOrders;
using TestService.Application.Order.Queries.GetOrdersWithPaging;
using TestService.Application.Order.DTOs;
using TestService.Api.Models;
using Wolverine;

namespace TestService.Api.Controllers;

[ApiController]
[Route("api/rest/order")]
[Produces("application/json")]
[Tags("Order Management")]
public class OrderController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IMessageBus messageBus, ILogger<OrderController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders with paging
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged list of orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> GetOrders(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting orders with page={Page}, pageSize={PageSize}", page, pageSize);
        
        var query = new GetOrdersWithPagingQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<PagedResult<OrderDto>>(query, cancellationToken);
        
        var response = new PagedResponse<OrderResponse>
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
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrderById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting order with id={Id}", id);
        
        var query = new GetOrderByIdQuery(id);
        var result = await _messageBus.InvokeAsync<OrderDto?>(query, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("Order with id={Id} not found", id);
            return NotFound();
        }
        
        return Ok(MapToResponse(result));
    }

    /// <summary>
    /// Create new order
    /// </summary>
    /// <param name="request">Create order request</param>
    /// <returns>Created order ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new order");
        
        var command = MapToCreateCommand(request);
        var id = await _messageBus.InvokeAsync<Guid>(command, cancellationToken);
        
        _logger.LogInformation("Order created with id={Id}", id);
        return CreatedAtAction(nameof(GetOrderById), new { id }, id);
    }

    /// <summary>
    /// Update existing order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Update order request</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrder(
        Guid id,
        [FromBody] UpdateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating order with id={Id}", id);
        
        var command = MapToUpdateCommand(id, request);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Order with id={Id} updated successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Delete order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting order with id={Id}", id);
        
        var command = new DeleteOrderCommand(id);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Order with id={Id} deleted successfully", id);
        return NoContent();
    }

    // Mapping methods
    private static OrderResponse MapToResponse(OrderDto dto)
    {
        return new OrderResponse
        {
            Id = dto.Id,
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    private static CreateOrderCommand MapToCreateCommand(CreateOrderRequest request)
    {
        return new CreateOrderCommand(
            request.Id,
            request.Name,
            request.Description,
            request.CreatedAt,
            request.UpdatedAt);
    }

    private static UpdateOrderCommand MapToUpdateCommand(Guid id, UpdateOrderRequest request)
    {
        return new UpdateOrderCommand(
            id,
            request.Id,
            request.Name,
            request.Description,
            request.CreatedAt,
            request.UpdatedAt);
    }
}