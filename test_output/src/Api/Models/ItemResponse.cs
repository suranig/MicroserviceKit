namespace Company.TestService.Api.Models;

/// <summary>
/// Response model for Item
/// </summary>
public class ItemResponse
{
    public Guid Id { get; set; }
public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}