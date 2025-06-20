using System.ComponentModel.DataAnnotations;

namespace OrderService.Api.Models;

/// <summary>
/// Request model for creating Order
/// </summary>
public class CreateOrderRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}