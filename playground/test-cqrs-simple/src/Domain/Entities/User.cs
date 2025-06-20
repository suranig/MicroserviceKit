using AggregateKit;
using SimpleService.Domain.Events;


namespace SimpleService.Domain.Entities;

public class User : AggregateRoot<Guid>
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private User() { } // For EF Core

    public User(Guid id, string name, string description) : base(Guid.NewGuid())
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
        
        AddDomainEvent(new UserCreatedEvent(Id));
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