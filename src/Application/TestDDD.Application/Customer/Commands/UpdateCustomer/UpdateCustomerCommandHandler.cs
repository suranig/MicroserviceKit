using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Customer.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler
{
    private readonly IRepository<Customer> _repository;

    public UpdateCustomerCommandHandler(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("Customer not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, cancellationToken);
    }
}