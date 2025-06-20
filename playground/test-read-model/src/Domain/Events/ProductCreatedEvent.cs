using AggregateKit;

namespace ReadModelService.Domain.Events;

public class ProductCreatedEvent : DomainEventBase
{
    public Guid AggregateId { get; }
    
    public ProductCreatedEvent(Guid aggregateId)
    {
        AggregateId = aggregateId;
    }
}