using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Order.Commands.UpdateOrder;

public class UpdateOrderCommandHandler
{
    private readonly IRepository<Order> _repository;

    public UpdateOrderCommandHandler(IRepository<Order> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("Order not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, cancellationToken);
    }
}