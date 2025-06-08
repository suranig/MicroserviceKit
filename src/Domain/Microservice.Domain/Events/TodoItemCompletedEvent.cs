using AggregateKit;

namespace Microservice.Domain.Events;

public class TodoItemCompletedEvent : DomainEventBase
{
    public Guid TodoItemId { get; }

    public TodoItemCompletedEvent(Guid todoItemId)
    {
        TodoItemId = todoItemId;
    }
} 