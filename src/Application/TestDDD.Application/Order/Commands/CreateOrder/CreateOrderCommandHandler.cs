using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Order.Commands.CreateOrder;

public class CreateOrderCommandHandler
{
    private readonly IRepository<Order> _repository;

    public CreateOrderCommandHandler(IRepository<Order> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var entity = new Order(command.CustomerId, command.TotalAmount, command.Status);
        await _repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}