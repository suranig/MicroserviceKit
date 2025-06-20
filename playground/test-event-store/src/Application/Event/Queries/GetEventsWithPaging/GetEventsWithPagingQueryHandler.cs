using EventStoreService.Application.Event.DTOs;
using EventStoreService.Application.Common;
using MassTransit;

namespace EventStoreService.Application.Event.Queries.GetEventsWithPaging;

public class GetEventsWithPagingQueryHandler : IConsumer<GetEventsWithPagingQuery>
{
    private readonly IRepository<EventStoreService.Domain.Entities.Event> _repository;

    public GetEventsWithPagingQueryHandler(IRepository<EventStoreService.Domain.Entities.Event> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetEventsWithPagingQuery> context)
    {
        var query = context.Message;
        var pagedResult = await _repository.GetPagedAsync(query.Page, query.PageSize, context.CancellationToken);
        var result = new PagedResult<EventDto>
        {
            Items = pagedResult.Items.Select(MapToDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
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