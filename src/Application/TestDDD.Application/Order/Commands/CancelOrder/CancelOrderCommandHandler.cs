using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Order.Commands.CancelOrder;

public class CancelOrderCommandHandler
{
    private readonly IRepository<Order> _repository;

    public CancelOrderCommandHandler(IRepository<Order> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implement Cancel logic
    }
}