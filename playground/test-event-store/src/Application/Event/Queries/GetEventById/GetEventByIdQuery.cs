using EventStoreService.Application.Event.DTOs;

namespace EventStoreService.Application.Event.Queries.GetEventById;

public record GetEventByIdQuery(Guid Id);