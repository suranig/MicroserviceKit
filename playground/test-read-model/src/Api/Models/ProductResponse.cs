namespace ReadModelService.Api.Models;

/// <summary>
/// Response model for Product
/// </summary>
public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}