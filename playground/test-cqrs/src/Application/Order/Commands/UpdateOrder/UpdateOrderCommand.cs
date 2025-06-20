namespace OrderService.Application.Order.Commands.UpdateOrder;

public record UpdateOrderCommand(Guid Id, string Name, string Description);