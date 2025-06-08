using AggregateKit;

namespace Microservice.Domain.Events;

public record TodoItemCompletedEvent(Guid TodoItemId) : DomainEventBase; 