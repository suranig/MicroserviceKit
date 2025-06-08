namespace ECommerce.OrderService.Application.Order.Commands.AddItemOrder;

public record AddItemOrderCommand(Guid customerId, decimal totalAmount);