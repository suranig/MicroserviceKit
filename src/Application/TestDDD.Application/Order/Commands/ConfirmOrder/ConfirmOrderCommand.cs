namespace ECommerce.OrderService.Application.Order.Commands.ConfirmOrder;

public record ConfirmOrderCommand(Guid customerId, decimal totalAmount);