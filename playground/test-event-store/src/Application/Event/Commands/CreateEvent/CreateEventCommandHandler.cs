using EventStoreService.Domain.Entities;
using EventStoreService.Application.Common;
using MassTransit;

namespace EventStoreService.Application.Event.Commands.CreateEvent;

public class CreateEventCommandHandler : IConsumer<CreateEventCommand>
{
    private readonly IRepository<EventStoreService.Domain.Entities.Event> _repository;

    public CreateEventCommandHandler(IRepository<EventStoreService.Domain.Entities.Event> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CreateEventCommand> context)
    {
        var command = context.Message;
        var entity = new EventStoreService.Domain.Entities.Event(Guid.NewGuid(), command.Name, command.Description);
        await _repository.AddAsync(entity, context.CancellationToken);
        
        await context.RespondAsync(entity.Id);
    }
}