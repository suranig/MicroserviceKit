using System.ComponentModel.DataAnnotations;

namespace ECommerce.OrderService.Api.Models;

/// <summary>
/// Request model for updating Order
/// </summary>
public class UpdateOrderRequest
{
public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
}