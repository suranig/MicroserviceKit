using AggregateKit;

namespace WorkflowService.Domain.Events;

public class WorkflowCreatedEvent : DomainEventBase
{
    public Guid AggregateId { get; }
    
    public WorkflowCreatedEvent(Guid aggregateId)
    {
        AggregateId = aggregateId;
    }
}