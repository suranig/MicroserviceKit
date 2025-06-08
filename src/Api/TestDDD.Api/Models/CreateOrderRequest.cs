using System.ComponentModel.DataAnnotations;

namespace ECommerce.OrderService.Api.Models;

/// <summary>
/// Request model for creating Order
/// </summary>
public class CreateOrderRequest
{
public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
}