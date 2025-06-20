using Microsoft.AspNetCore.Mvc;
using TestService.Application.Test.Commands.CreateTest;
using TestService.Application.Test.Commands.UpdateTest;
using TestService.Application.Test.Commands.DeleteTest;
using TestService.Application.Test.Queries.GetTestById;
using TestService.Application.Test.Queries.GetTests;
using TestService.Application.Test.Queries.GetTestsWithPaging;
using TestService.Application.Test.DTOs;
using TestService.Api.Models;
using Wolverine;

namespace TestService.Api.Controllers;

[ApiController]
[Route("api/rest/test")]
[Produces("application/json")]
[Tags("Test Management")]
public class TestController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<TestController> _logger;

    public TestController(IMessageBus messageBus, ILogger<TestController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Get all tests with paging
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged list of tests</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TestResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<TestResponse>>> GetTests(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting tests with page={Page}, pageSize={PageSize}", page, pageSize);
        
        var query = new GetTestsWithPagingQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<PagedResult<TestDto>>(query, cancellationToken);
        
        var response = new PagedResponse<TestResponse>
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
    /// Get test by ID
    /// </summary>
    /// <param name="id">Test ID</param>
    /// <returns>Test details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestResponse>> GetTestById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting test with id={Id}", id);
        
        var query = new GetTestByIdQuery(id);
        var result = await _messageBus.InvokeAsync<TestDto?>(query, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("Test with id={Id} not found", id);
            return NotFound();
        }
        
        return Ok(MapToResponse(result));
    }

    /// <summary>
    /// Create new test
    /// </summary>
    /// <param name="request">Create test request</param>
    /// <returns>Created test ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateTest(
        [FromBody] CreateTestRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new test");
        
        var command = MapToCreateCommand(request);
        var id = await _messageBus.InvokeAsync<Guid>(command, cancellationToken);
        
        _logger.LogInformation("Test created with id={Id}", id);
        return CreatedAtAction(nameof(GetTestById), new { id }, id);
    }

    /// <summary>
    /// Update existing test
    /// </summary>
    /// <param name="id">Test ID</param>
    /// <param name="request">Update test request</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTest(
        Guid id,
        [FromBody] UpdateTestRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating test with id={Id}", id);
        
        var command = MapToUpdateCommand(id, request);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Test with id={Id} updated successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Delete test
    /// </summary>
    /// <param name="id">Test ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTest(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting test with id={Id}", id);
        
        var command = new DeleteTestCommand(id);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Test with id={Id} deleted successfully", id);
        return NoContent();
    }

    // Mapping methods
    private static TestResponse MapToResponse(TestDto dto)
    {
        return new TestResponse
        {
            Id = dto.Id,
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    private static CreateTestCommand MapToCreateCommand(CreateTestRequest request)
    {
        return new CreateTestCommand(
            request.Id,
            request.Name,
            request.Description,
            request.CreatedAt,
            request.UpdatedAt);
    }

    private static UpdateTestCommand MapToUpdateCommand(Guid id, UpdateTestRequest request)
    {
        return new UpdateTestCommand(
            id,
            request.Id,
            request.Name,
            request.Description,
            request.CreatedAt,
            request.UpdatedAt);
    }
}