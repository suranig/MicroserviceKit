namespace ECommerce.OrderService.Application.Order.DTOs;

public class UpdateOrderDto
{
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
}