using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Order.Commands.DeleteOrder;

public class DeleteOrderCommandHandler
{
    private readonly IRepository<Order> _repository;

    public DeleteOrderCommandHandler(IRepository<Order> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("Order not found");
            
        await _repository.DeleteAsync(entity, cancellationToken);
    }
}