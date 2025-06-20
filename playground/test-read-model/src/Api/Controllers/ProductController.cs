using Microsoft.AspNetCore.Mvc;
using ReadModelService.Application.Product.Commands.CreateProduct;
using ReadModelService.Application.Product.Commands.UpdateProduct;
using ReadModelService.Application.Product.Commands.DeleteProduct;
using ReadModelService.Application.Product.Queries.GetProductById;
using ReadModelService.Application.Product.Queries.GetProducts;
using ReadModelService.Application.Product.Queries.GetProductsWithPaging;
using ReadModelService.Application.Product.DTOs;
using ReadModelService.Api.Models;
using Wolverine;

namespace ReadModelService.Api.Controllers;

[ApiController]
[Route("api/rest/product")]
[Produces("application/json")]
[Tags("Product Management")]
public class ProductController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IMessageBus messageBus, ILogger<ProductController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with paging
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetProducts(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting products with page={Page}, pageSize={PageSize}", page, pageSize);
        
        var query = new GetProductsWithPagingQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<PagedResult<ProductDto>>(query, cancellationToken);
        
        var response = new PagedResponse<ProductResponse>
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
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProductById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product with id={Id}", id);
        
        var query = new GetProductByIdQuery(id);
        var result = await _messageBus.InvokeAsync<ProductDto?>(query, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("Product with id={Id} not found", id);
            return NotFound();
        }
        
        return Ok(MapToResponse(result));
    }

    /// <summary>
    /// Create new product
    /// </summary>
    /// <param name="request">Create product request</param>
    /// <returns>Created product ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new product");
        
        var command = MapToCreateCommand(request);
        var id = await _messageBus.InvokeAsync<Guid>(command, cancellationToken);
        
        _logger.LogInformation("Product created with id={Id}", id);
        return CreatedAtAction(nameof(GetProductById), new { id }, id);
    }

    /// <summary>
    /// Update existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Update product request</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product with id={Id}", id);
        
        var command = MapToUpdateCommand(id, request);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Product with id={Id} updated successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Delete product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product with id={Id}", id);
        
        var command = new DeleteProductCommand(id);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Product with id={Id} deleted successfully", id);
        return NoContent();
    }

    // Mapping methods
    private static ProductResponse MapToResponse(ProductDto dto)
    {
        return new ProductResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description
        };
    }

    private static CreateProductCommand MapToCreateCommand(CreateProductRequest request)
    {
        return new CreateProductCommand(
            request.Id,
            request.Name,
            request.Description);
    }

    private static UpdateProductCommand MapToUpdateCommand(Guid id, UpdateProductRequest request)
    {
        return new UpdateProductCommand(
            id,
            request.Id,
            request.Name,
            request.Description);
    }
}