using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Customer.Commands.DeleteCustomer;

public class DeleteCustomerCommandHandler
{
    private readonly IRepository<Customer> _repository;

    public DeleteCustomerCommandHandler(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(DeleteCustomerCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("Customer not found");
            
        await _repository.DeleteAsync(entity, cancellationToken);
    }
}