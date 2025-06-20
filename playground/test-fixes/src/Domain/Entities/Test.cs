using AggregateKit;
using TestService.Domain.Events;

namespace TestService.Domain.Entities;

public class Test : AggregateRoot<Guid>
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Test() { } // For EF Core

    public Test(Guid id, string name, string description, DateTime createdat, DateTime updatedat) : base(Guid.NewGuid())
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = createdat;
        UpdatedAt = updatedat;
        
        AddDomainEvent(new TestCreatedEvent(Id));
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