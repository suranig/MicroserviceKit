using Microsoft.AspNetCore.Mvc;
using ECommerce.OrderService.Application.Customer.Commands.CreateCustomer;
using ECommerce.OrderService.Application.Customer.Commands.UpdateCustomer;
using ECommerce.OrderService.Application.Customer.Commands.DeleteCustomer;
using ECommerce.OrderService.Application.Customer.Queries.GetCustomerById;
using ECommerce.OrderService.Application.Customer.Queries.GetCustomers;
using ECommerce.OrderService.Application.Customer.Queries.GetCustomersWithPaging;
using ECommerce.OrderService.Application.Customer.DTOs;
using ECommerce.OrderService.Api.Models;
using Wolverine;

namespace ECommerce.OrderService.Api.Controllers;

[ApiController]
[Route("api/rest/customer")]
[Produces("application/json")]
[Tags("Customer Management")]
public class CustomerController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(IMessageBus messageBus, ILogger<CustomerController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers with paging
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged list of customers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<CustomerResponse>>> GetCustomers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting customers with page={Page}, pageSize={PageSize}", page, pageSize);
        
        var query = new GetCustomersWithPagingQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<PagedResult<CustomerDto>>(query, cancellationToken);
        
        var response = new PagedResponse<CustomerResponse>
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
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetCustomerById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting customer with id={Id}", id);
        
        var query = new GetCustomerByIdQuery(id);
        var result = await _messageBus.InvokeAsync<CustomerDto?>(query, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("Customer with id={Id} not found", id);
            return NotFound();
        }
        
        return Ok(MapToResponse(result));
    }

    /// <summary>
    /// Create new customer
    /// </summary>
    /// <param name="request">Create customer request</param>
    /// <returns>Created customer ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new customer");
        
        var command = MapToCreateCommand(request);
        var id = await _messageBus.InvokeAsync<Guid>(command, cancellationToken);
        
        _logger.LogInformation("Customer created with id={Id}", id);
        return CreatedAtAction(nameof(GetCustomerById), new { id }, id);
    }

    /// <summary>
    /// Update existing customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Update customer request</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating customer with id={Id}", id);
        
        var command = MapToUpdateCommand(id, request);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Customer with id={Id} updated successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Delete customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting customer with id={Id}", id);
        
        var command = new DeleteCustomerCommand(id);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Customer with id={Id} deleted successfully", id);
        return NoContent();
    }

    // Mapping methods
    private static CustomerResponse MapToResponse(CustomerDto dto)
    {
        return new CustomerResponse
        {
            Id = dto.Id,
            Email = dto.Email,
            Name = dto.Name
        };
    }

    private static CreateCustomerCommand MapToCreateCommand(CreateCustomerRequest request)
    {
        return new CreateCustomerCommand(
            request.Email,
            request.Name);
    }

    private static UpdateCustomerCommand MapToUpdateCommand(Guid id, UpdateCustomerRequest request)
    {
        return new UpdateCustomerCommand(
            id,
            request.Email,
            request.Name);
    }
}