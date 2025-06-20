using EventStoreService.Application.Event.DTOs;
using EventStoreService.Application.Common;
using MassTransit;

namespace EventStoreService.Application.Event.Queries.GetEventById;

public class GetEventByIdQueryHandler : IConsumer<GetEventByIdQuery>
{
    private readonly IRepository<EventStoreService.Domain.Entities.Event> _repository;

    public GetEventByIdQueryHandler(IRepository<EventStoreService.Domain.Entities.Event> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetEventByIdQuery> context)
    {
        var query = context.Message;
        var entity = await _repository.GetByIdAsync(query.Id, context.CancellationToken);
        var result = entity == null ? null : MapToDto(entity);
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