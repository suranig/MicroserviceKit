using TestService.Domain.Enums;
namespace TestService.Application.Order.Commands.CancelOrder;

public record CancelOrderCommand(Guid Id);