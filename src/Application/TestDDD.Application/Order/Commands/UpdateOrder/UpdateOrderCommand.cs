namespace ECommerce.OrderService.Application.Order.Commands.UpdateOrder;

public record UpdateOrderCommand(Guid id, Guid customerId, decimal totalAmount, string status);