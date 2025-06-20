using System.ComponentModel.DataAnnotations;

namespace TestService.Api.Models;

/// <summary>
/// Request model for creating Order
/// </summary>
public class CreateOrderRequest
{
public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
}