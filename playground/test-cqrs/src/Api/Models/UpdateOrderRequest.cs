using System.ComponentModel.DataAnnotations;

namespace OrderService.Api.Models;

/// <summary>
/// Request model for updating Order
/// </summary>
public class UpdateOrderRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}