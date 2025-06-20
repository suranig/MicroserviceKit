using TestService.Domain.Enums;
namespace TestService.Application.Order.Commands.DeleteOrder;

public record DeleteOrderCommand(Guid id);