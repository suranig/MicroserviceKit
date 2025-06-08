using System.ComponentModel.DataAnnotations;

namespace ECommerce.OrderService.Api.Models;

/// <summary>
/// Request model for creating Customer
/// </summary>
public class CreateCustomerRequest
{
public string Email { get; set; }
    public string Name { get; set; }
}