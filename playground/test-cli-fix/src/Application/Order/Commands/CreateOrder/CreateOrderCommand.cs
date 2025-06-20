using TestService.Domain.Enums;
namespace TestService.Application.Order.Commands.CreateOrder;

public record CreateOrderCommand(Guid CustomerId, decimal TotalAmount, OrderStatus Status);