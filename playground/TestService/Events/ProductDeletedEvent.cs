using AggregateKit;

namespace TestService.Domain.Events;

public record ProductDeletedEvent(Guid ProductId) : DomainEventBase;