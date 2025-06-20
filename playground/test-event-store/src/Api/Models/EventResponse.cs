namespace EventStoreService.Api.Models;

/// <summary>
/// Response model for Event
/// </summary>
public class EventResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}