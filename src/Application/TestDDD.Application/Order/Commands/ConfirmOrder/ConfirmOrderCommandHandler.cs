using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Order.Commands.ConfirmOrder;

public class ConfirmOrderCommandHandler
{
    private readonly IRepository<Order> _repository;

    public ConfirmOrderCommandHandler(IRepository<Order> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(ConfirmOrderCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implement Confirm logic
    }
}