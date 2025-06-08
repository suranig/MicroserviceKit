using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Order.Commands.RemoveItemOrder;

public class RemoveItemOrderCommandHandler
{
    private readonly IRepository<Order> _repository;

    public RemoveItemOrderCommandHandler(IRepository<Order> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(RemoveItemOrderCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implement RemoveItem logic
    }
}