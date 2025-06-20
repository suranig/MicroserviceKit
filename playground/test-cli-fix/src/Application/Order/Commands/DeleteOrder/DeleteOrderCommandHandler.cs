using TestService.Domain.Entities;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Order.Commands.DeleteOrder;

public class DeleteOrderCommandHandler : IConsumer<DeleteOrderCommand>
{
    private readonly IRepository<TestService.Domain.Entities.Order> _repository;

    public DeleteOrderCommandHandler(IRepository<TestService.Domain.Entities.Order> repository)
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