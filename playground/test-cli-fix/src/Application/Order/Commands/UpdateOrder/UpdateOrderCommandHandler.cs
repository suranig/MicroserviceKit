using TestService.Domain.Entities;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Order.Commands.UpdateOrder;

public class UpdateOrderCommandHandler : IConsumer<UpdateOrderCommand>
{
    private readonly IRepository<TestService.Domain.Entities.Order> _repository;

    public UpdateOrderCommandHandler(IRepository<TestService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<UpdateOrderCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.Id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("Order not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, context.CancellationToken);
        
        
    }
}