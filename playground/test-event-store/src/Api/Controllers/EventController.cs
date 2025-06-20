using Microsoft.AspNetCore.Mvc;
using EventStoreService.Application.Event.Commands.CreateEvent;
using EventStoreService.Application.Event.Commands.UpdateEvent;
using EventStoreService.Application.Event.Commands.DeleteEvent;
using EventStoreService.Application.Event.Queries.GetEventById;
using EventStoreService.Application.Event.Queries.GetEvents;
using EventStoreService.Application.Event.Queries.GetEventsWithPaging;
using EventStoreService.Application.Event.DTOs;
using EventStoreService.Api.Models;
using MassTransit;

namespace EventStoreService.Api.Controllers;

[ApiController]
[Route("api/rest/event")]
[Produces("application/json")]
[Tags("Event Management")]
public class EventController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<EventController> _logger;

    public EventController(IMessageBus messageBus, ILogger<EventController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Get all events with paging
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paged list of events</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<EventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<EventResponse>>> GetEvents(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting events with page={Page}, pageSize={PageSize}", page, pageSize);
        
        var query = new GetEventsWithPagingQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<PagedResponse<EventDto>>(query, cancellationToken);
        
        var response = new PagedResponse<EventResponse>
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
    /// Get event by ID
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>Event details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetEventById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting event with id={Id}", id);
        
        var query = new GetEventByIdQuery(id);
        var result = await _messageBus.InvokeAsync<EventDto?>(query, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("Event with id={Id} not found", id);
            return NotFound();
        }
        
        return Ok(MapToResponse(result));
    }

    /// <summary>
    /// Create new event
    /// </summary>
    /// <param name="request">Create event request</param>
    /// <returns>Created event ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateEvent(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new event");
        
        var command = MapToCreateCommand(request);
        var id = await _messageBus.InvokeAsync<Guid>(command, cancellationToken);
        
        _logger.LogInformation("Event created with id={Id}", id);
        return CreatedAtAction(nameof(GetEventById), new { id }, id);
    }

    /// <summary>
    /// Update existing event
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <param name="request">Update event request</param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(
        Guid id,
        [FromBody] UpdateEventRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating event with id={Id}", id);
        
        var command = MapToUpdateCommand(id, request);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Event with id={Id} updated successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Delete event
    /// </summary>
    /// <param name="id">Event ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting event with id={Id}", id);
        
        var command = new DeleteEventCommand(id);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation("Event with id={Id} deleted successfully", id);
        return NoContent();
    }

    // Mapping methods
    private static EventResponse MapToResponse(EventDto dto)
    {
        return new EventResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description
        };
    }

    private static CreateEventCommand MapToCreateCommand(CreateEventRequest request)
    {
        return new CreateEventCommand(
            request.Name,
            request.Description);
    }

    private static UpdateEventCommand MapToUpdateCommand(Guid id, UpdateEventRequest request)
    {
        return new UpdateEventCommand(
            id,
            request.Name,
            request.Description);
    }
}