namespace TestService.Api.Models;

/// <summary>
/// Response model for Order
/// </summary>
public class OrderResponse
{
    public Guid Id { get; set; }
public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}