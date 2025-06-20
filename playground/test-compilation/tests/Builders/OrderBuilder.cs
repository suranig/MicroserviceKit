using TestService.Domain.Entities;

namespace TestService.UnitTests.Builders;

public class OrderBuilder
{
    private Guid _id = Guid.NewGuid();
private Guid _id = Guid.NewGuid();
    private string _name = "Test Value";
    private string _description = "Test Value";
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public OrderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public OrderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public OrderBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public OrderBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public OrderBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public OrderBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public Order Build()
    {
        var entity = new Order(_id, _name, _description, _createdAt, _updatedAt);
        
        // Use reflection to set the Id if needed
        var idProperty = typeof(Order).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, _id);
        }
        
        return entity;
    }

    public static implicit operator Order(OrderBuilder builder)
    {
        return builder.Build();
    }
}