namespace ECommerce.OrderService.Application.Customer.Commands.UpdateCustomer;

public record UpdateCustomerCommand(Guid id, string email, string name);