using AggregateKit;

namespace TestService.Domain.Events;

public class TestCreatedEvent : DomainEventBase
{
    public Guid AggregateId { get; }
    
    public TestCreatedEvent(Guid aggregateId)
    {
        AggregateId = aggregateId;
    }
}