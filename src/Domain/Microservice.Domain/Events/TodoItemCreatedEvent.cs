using AggregateKit;

namespace Microservice.Domain.Events;

public record TodoItemCreatedEvent(Guid TodoItemId, string Title) : DomainEventBase; 