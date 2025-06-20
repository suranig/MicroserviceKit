using AggregateKit;
using WorkflowService.Domain.Events;


namespace WorkflowService.Domain.Entities;

public class Workflow : AggregateRoot<Guid>
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Workflow() { } // For EF Core

    public Workflow(Guid id, string name, string description) : base(Guid.NewGuid())
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
        
        AddDomainEvent(new WorkflowCreatedEvent(Id));
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