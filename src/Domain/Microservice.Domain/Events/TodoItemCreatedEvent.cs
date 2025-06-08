using AggregateKit;

namespace Microservice.Domain.Events;

public class TodoItemCreatedEvent : DomainEventBase
{
    public Guid TodoItemId { get; }
    public string Title { get; }

    public TodoItemCreatedEvent(Guid todoItemId, string title)
    {
        TodoItemId = todoItemId;
        Title = title;
    }
} 