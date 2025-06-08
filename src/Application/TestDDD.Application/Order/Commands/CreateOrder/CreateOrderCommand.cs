namespace ECommerce.OrderService.Application.Order.Commands.CreateOrder;

public record CreateOrderCommand(Guid customerId, decimal totalAmount, string status);