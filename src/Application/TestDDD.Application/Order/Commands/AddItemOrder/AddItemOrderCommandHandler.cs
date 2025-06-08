using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Order.Commands.AddItemOrder;

public class AddItemOrderCommandHandler
{
    private readonly IRepository<Order> _repository;

    public AddItemOrderCommandHandler(IRepository<Order> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(AddItemOrderCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implement AddItem logic
    }
}