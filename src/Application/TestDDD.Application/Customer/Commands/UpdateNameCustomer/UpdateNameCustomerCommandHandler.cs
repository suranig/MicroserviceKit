using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Customer.Commands.UpdateNameCustomer;

public class UpdateNameCustomerCommandHandler
{
    private readonly IRepository<Customer> _repository;

    public UpdateNameCustomerCommandHandler(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(UpdateNameCustomerCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implement UpdateName logic
    }
}