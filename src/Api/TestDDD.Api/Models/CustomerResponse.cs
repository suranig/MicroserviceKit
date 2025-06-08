namespace ECommerce.OrderService.Api.Models;

/// <summary>
/// Response model for Customer
/// </summary>
public class CustomerResponse
{
    public Guid Id { get; set; }
public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}