using AggregateKit;

namespace SimpleService.Domain.Events;

public record UserUpdatedEvent(Guid UserId, Guid Id, string Name, string Description) : DomainEventBase;