using AggregateKit;
using TestService.Domain.Events;
using TestService.Domain.Enums;
using TestService.Domain.ValueObjects;

namespace TestService.Domain.Entities;

public class Order : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Order() { } // For EF Core

    public Order(Guid customerid, decimal totalamount, OrderStatus status) : base(Guid.NewGuid())
    {
        CustomerId = customerid;
        TotalAmount = totalamount;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
        
        AddDomainEvent(new OrderCreatedEvent(Id));
    }

    public void Create()
    {
        // TODO: Implement Create
    }

    public void AddItem()
    {
        // TODO: Implement AddItem
    }

    public void RemoveItem()
    {
        // TODO: Implement RemoveItem
    }

    public void Confirm()
    {
        // TODO: Implement Confirm
    }

    public void Cancel()
    {
        // TODO: Implement Cancel
    }
}