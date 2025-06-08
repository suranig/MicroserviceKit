namespace ECommerce.OrderService.Api.Models;

/// <summary>
/// Response model for Order
/// </summary>
public class OrderResponse
{
    public Guid Id { get; set; }
public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}