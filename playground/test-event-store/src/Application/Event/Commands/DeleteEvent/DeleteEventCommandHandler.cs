using EventStoreService.Domain.Entities;
using EventStoreService.Application.Common;
using MassTransit;

namespace EventStoreService.Application.Event.Commands.DeleteEvent;

public class DeleteEventCommandHandler : IConsumer<DeleteEventCommand>
{
    private readonly IRepository<EventStoreService.Domain.Entities.Event> _repository;

    public DeleteEventCommandHandler(IRepository<EventStoreService.Domain.Entities.Event> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<DeleteEventCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("Event not found");
            
        await _repository.DeleteAsync(entity, context.CancellationToken);
        
        
    }
}