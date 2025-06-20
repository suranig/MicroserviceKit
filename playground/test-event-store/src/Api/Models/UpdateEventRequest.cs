using System.ComponentModel.DataAnnotations;

namespace EventStoreService.Api.Models;

/// <summary>
/// Request model for updating Event
/// </summary>
public class UpdateEventRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}