using AggregateKit;
using Company.TestService.Domain.Events;

namespace Company.TestService.Domain.Entities;

public class Item : AggregateRoot<Guid>
{
    public string Title { get; private set; }
    public bool IsCompleted { get; private set; }

    private Item() { } // For EF Core

    public Item(string title, bool iscompleted) : base(Guid.NewGuid())
    {
        Title = title;
        IsCompleted = iscompleted;
        
        AddDomainEvent(new ItemCreatedEvent(Id));
    }

    public void Create()
    {
        // TODO: Implement Create
    }

    public void MarkComplete()
    {
        // TODO: Implement MarkComplete
    }
}