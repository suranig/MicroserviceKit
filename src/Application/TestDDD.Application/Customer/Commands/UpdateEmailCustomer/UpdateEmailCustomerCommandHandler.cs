using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Customer.Commands.UpdateEmailCustomer;

public class UpdateEmailCustomerCommandHandler
{
    private readonly IRepository<Customer> _repository;

    public UpdateEmailCustomerCommandHandler(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(UpdateEmailCustomerCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implement UpdateEmail logic
    }
}