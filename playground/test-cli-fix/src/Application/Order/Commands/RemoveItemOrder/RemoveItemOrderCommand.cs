using TestService.Domain.Enums;
namespace TestService.Application.Order.Commands.RemoveItemOrder;

public record RemoveItemOrderCommand(Guid Id);