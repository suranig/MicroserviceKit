using AggregateKit;
using Microservice.Domain.Events;

namespace Microservice.Domain.Entities;

public class TodoItem : AggregateRoot<Guid>
{
    public string Title { get; private set; }
    public bool IsCompleted { get; private set; }

    private TodoItem() { } // For EF Core

    public TodoItem(string title) : base(Guid.NewGuid())
    {
        Title = title;
        IsCompleted = false;
        
        AddDomainEvent(new TodoItemCreatedEvent(Id, Title));
    }

    public void MarkComplete()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            AddDomainEvent(new TodoItemCompletedEvent(Id));
        }
    }
}
