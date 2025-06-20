using AggregateKit;

namespace TestService.Domain.Events;

public record TestUpdatedEvent(Guid TestId, Guid Id, string Name, string Description, DateTime CreatedAt, DateTime UpdatedAt) : DomainEventBase;