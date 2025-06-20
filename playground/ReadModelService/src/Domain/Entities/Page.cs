using AggregateKit;
using ReadModelService.Domain.Events;

namespace ReadModelService.Domain.Entities;

public class Page : AggregateRoot<Guid>
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Page() { } // For EF Core

    public Page(Guid id, DateTime createdat, DateTime updatedat) : base(Guid.NewGuid())
    {
        Id = id;
        CreatedAt = createdat;
        UpdatedAt = updatedat;
        
        AddDomainEvent(new PageCreatedEvent(Id));
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