using EventStoreService.Application.Event.DTOs;

namespace EventStoreService.Application.Event.Queries.GetEventsWithPaging;

public record GetEventsWithPagingQuery(int Page = 1, int PageSize = 10);