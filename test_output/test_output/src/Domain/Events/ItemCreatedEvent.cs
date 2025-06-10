using AggregateKit;

namespace Company.TestService.Domain.Events;

public class ItemCreatedEvent : DomainEventBase
{
    public Guid AggregateId { get; }
    
    public ItemCreatedEvent(Guid aggregateId)
    {
        AggregateId = aggregateId;
    }
}