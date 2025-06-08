using System.ComponentModel.DataAnnotations;

namespace ECommerce.OrderService.Api.Models;

/// <summary>
/// Request model for updating Customer
/// </summary>
public class UpdateCustomerRequest
{
public string Email { get; set; }
    public string Name { get; set; }
}