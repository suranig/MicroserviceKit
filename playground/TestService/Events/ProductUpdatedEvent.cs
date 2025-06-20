using AggregateKit;

namespace TestService.Domain.Events;

public record ProductUpdatedEvent(Guid ProductId, Guid Id, DateTime CreatedAt, DateTime UpdatedAt) : DomainEventBase;