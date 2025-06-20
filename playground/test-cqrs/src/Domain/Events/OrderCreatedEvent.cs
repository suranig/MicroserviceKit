using AggregateKit;

namespace OrderService.Domain.Events;

public class OrderCreatedEvent : DomainEventBase
{
    public Guid AggregateId { get; }
    
    public OrderCreatedEvent(Guid aggregateId)
    {
        AggregateId = aggregateId;
    }
}