using AggregateKit;

namespace SimpleService.Domain.Events;

public class UserCreatedEvent : DomainEventBase
{
    public Guid AggregateId { get; }
    
    public UserCreatedEvent(Guid aggregateId)
    {
        AggregateId = aggregateId;
    }
}