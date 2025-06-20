using TestService.Domain.Enums;
namespace TestService.Application.Order.Commands.UpdateOrder;

public record UpdateOrderCommand(Guid Id, Guid CustomerId, decimal TotalAmount, OrderStatus Status);