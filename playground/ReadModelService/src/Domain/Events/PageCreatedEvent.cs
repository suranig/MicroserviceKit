using AggregateKit;

namespace ReadModelService.Domain.Events;

public class PageCreatedEvent : DomainEventBase
{
    public Guid AggregateId { get; }
    
    public PageCreatedEvent(Guid aggregateId)
    {
        AggregateId = aggregateId;
    }
}