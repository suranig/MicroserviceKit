using Microsoft.AspNetCore.Mvc;
using SimpleService.Application.User.Commands.CreateUser;
using SimpleService.Application.User.Commands.UpdateUser;
using SimpleService.Application.User.Commands.DeleteUser;
using SimpleService.Application.User.Queries.GetUserById;
using SimpleService.Application.User.Queries.GetUsers;
using SimpleService.Application.User.Queries.GetUsersWithPaging;
using SimpleService.Application.User.DTOs;
using SimpleService.Api.Models;
using Wolverine;

namespace SimpleService.Api.Controllers;

[ApiController]
[Route("api/rest/user")]
[Produces("application/json")]
[Tags("User Management")]
public class UserController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<UserController> _logger;

    public UserController(IMessageBus messageBus, ILogger<UserController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Get all users with paging
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged list of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetUsers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting users with page={Page}, pageSize={PageSize}", page, pageSize);
        
        var query = new GetUsersWithPagingQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<PagedResult<UserDto>>(query, cancellationToken);
        
        var response = new PagedResponse<UserResponse>
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
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUserById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user with id={Id}", id);
        
        var query = new GetUserByIdQuery(id);
        var result = await _messageBus.InvokeAsync<UserDto?>(query, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("User with id={Id} not found", id);
            return NotFound();
        }
        
        return Ok(MapToResponse(result));
    }

    /// <summary>
    /// Create new user
    /// </summary>
    /// <param name="request">Create user request</param>
    /// <returns>Created user ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new user");
        
        var command = MapToCreateCommand(request);
        var id = await _messageBus.InvokeAsync<Guid>(command, cancellationToken);
        
        _logger.LogInformation("User created with id={Id}", id);
        return CreatedAtAction(nameof(GetUserById), new { id }, id);
    }

    /// <summary>
    /// Update existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Update user request</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user with id={Id}", id);
        
        var command = MapToUpdateCommand(id, request);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("User with id={Id} updated successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Delete user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user with id={Id}", id);
        
        var command = new DeleteUserCommand(id);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("User with id={Id} deleted successfully", id);
        return NoContent();
    }

    // Mapping methods
    private static UserResponse MapToResponse(UserDto dto)
    {
        return new UserResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description
        };
    }

    private static CreateUserCommand MapToCreateCommand(CreateUserRequest request)
    {
        return new CreateUserCommand(
            request.Id,
            request.Name,
            request.Description);
    }

    private static UpdateUserCommand MapToUpdateCommand(Guid id, UpdateUserRequest request)
    {
        return new UpdateUserCommand(
            id,
            request.Id,
            request.Name,
            request.Description);
    }
}