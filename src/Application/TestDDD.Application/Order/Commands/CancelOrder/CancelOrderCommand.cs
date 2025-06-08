namespace ECommerce.OrderService.Application.Order.Commands.CancelOrder;

public record CancelOrderCommand(Guid customerId, decimal totalAmount);