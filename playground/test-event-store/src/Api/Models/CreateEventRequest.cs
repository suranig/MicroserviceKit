using System.ComponentModel.DataAnnotations;

namespace EventStoreService.Api.Models;

/// <summary>
/// Request model for creating Event
/// </summary>
public class CreateEventRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}