using AggregateKit;

namespace TestService.Domain.Events;

public record TestDeletedEvent(Guid TestId) : DomainEventBase;