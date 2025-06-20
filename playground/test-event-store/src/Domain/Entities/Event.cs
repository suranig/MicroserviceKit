using AggregateKit;
using EventStoreService.Domain.Events;


namespace EventStoreService.Domain.Entities;

public class Event : AggregateRoot<Guid>
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Event() { } // For EF Core

    public Event(Guid id, string name, string description) : base(Guid.NewGuid())
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
        
        AddDomainEvent(new EventCreatedEvent(Id));
    }

    public void Create()
    {
        // TODO: Implement Create
    }

    public void Update()
    {
        // TODO: Implement Update
    }

    public void Delete()
    {
        // TODO: Implement Delete
    }
}