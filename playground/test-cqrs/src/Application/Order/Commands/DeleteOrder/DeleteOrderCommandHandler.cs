using OrderService.Domain.Entities;
using OrderService.Application.Common;
using MassTransit;

namespace OrderService.Application.Order.Commands.DeleteOrder;

public class DeleteOrderCommandHandler : IConsumer<DeleteOrderCommand>
{
    private readonly IRepository<OrderService.Domain.Entities.Order> _repository;

    public DeleteOrderCommandHandler(IRepository<OrderService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<DeleteOrderCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("Order not found");
            
        await _repository.DeleteAsync(entity, context.CancellationToken);
        
        
    }
}