using AggregateKit;

namespace SimpleService.Domain.Events;

public record UserDeletedEvent(Guid UserId) : DomainEventBase;