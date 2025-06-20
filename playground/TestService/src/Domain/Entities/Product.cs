using AggregateKit;
using TestService.Domain.Events;

namespace TestService.Domain.Entities;

public class Product : AggregateRoot<Guid>
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Product() { } // For EF Core

    public Product(Guid id, DateTime createdat, DateTime updatedat) : base(Guid.NewGuid())
    {
        Id = id;
        CreatedAt = createdat;
        UpdatedAt = updatedat;
        
        AddDomainEvent(new ProductCreatedEvent(Id));
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