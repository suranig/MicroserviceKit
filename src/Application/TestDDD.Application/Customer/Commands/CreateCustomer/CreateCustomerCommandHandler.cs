using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Customer.Commands.CreateCustomer;

public class CreateCustomerCommandHandler
{
    private readonly IRepository<Customer> _repository;

    public CreateCustomerCommandHandler(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var entity = new Customer(command.Email, command.Name);
        await _repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}