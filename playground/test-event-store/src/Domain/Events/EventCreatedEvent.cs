using AggregateKit;

namespace EventStoreService.Domain.Events;

public class EventCreatedEvent : DomainEventBase
{
    public Guid AggregateId { get; }
    
    public EventCreatedEvent(Guid aggregateId)
    {
        AggregateId = aggregateId;
    }
}