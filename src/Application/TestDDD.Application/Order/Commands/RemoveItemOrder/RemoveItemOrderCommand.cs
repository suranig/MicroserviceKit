namespace ECommerce.OrderService.Application.Order.Commands.RemoveItemOrder;

public record RemoveItemOrderCommand(Guid customerId, decimal totalAmount);