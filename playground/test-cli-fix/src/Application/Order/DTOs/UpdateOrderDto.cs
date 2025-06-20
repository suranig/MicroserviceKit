using TestService.Domain.Enums;
namespace TestService.Application.Order.DTOs;

public class UpdateOrderDto
{
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
}