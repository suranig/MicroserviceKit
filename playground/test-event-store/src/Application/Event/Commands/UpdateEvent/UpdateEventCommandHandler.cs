using EventStoreService.Domain.Entities;
using EventStoreService.Application.Common;
using MassTransit;

namespace EventStoreService.Application.Event.Commands.UpdateEvent;

public class UpdateEventCommandHandler : IConsumer<UpdateEventCommand>
{
    private readonly IRepository<EventStoreService.Domain.Entities.Event> _repository;

    public UpdateEventCommandHandler(IRepository<EventStoreService.Domain.Entities.Event> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<UpdateEventCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.Id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("Event not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, context.CancellationToken);
        
        
    }
}