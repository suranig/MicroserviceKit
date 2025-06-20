using EventStoreService.Application.Event.DTOs;
using EventStoreService.Application.Common;
using MassTransit;

namespace EventStoreService.Application.Event.Queries.GetEvents;

public class GetEventsQueryHandler : IConsumer<GetEventsQuery>
{
    private readonly IRepository<EventStoreService.Domain.Entities.Event> _repository;

    public GetEventsQueryHandler(IRepository<EventStoreService.Domain.Entities.Event> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetEventsQuery> context)
    {
        var query = context.Message;
        var entities = await _repository.GetAllAsync(context.CancellationToken);
        var result = entities.Select(MapToDto).ToList();
        await context.RespondAsync(result);
    }

    private EventDto MapToDto(EventStoreService.Domain.Entities.Event entity)
    {
        return new EventDto
        {
            Id = entity.Id,
            // TODO: Map other properties
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}