using TestService.Domain.Enums;
namespace TestService.Application.Order.Commands.ConfirmOrder;

public record ConfirmOrderCommand(Guid Id);