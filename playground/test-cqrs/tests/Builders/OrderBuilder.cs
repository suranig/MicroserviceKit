using OrderService.Domain.Entities;

namespace OrderService.UnitTests.Builders;

public class OrderBuilder
{
    private Guid _id = Guid.NewGuid();
private Guid _id = Guid.NewGuid();
    private string _name = "Test Value";
    private string _description = "Test Value";

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

    public Order Build()
    {
        var entity = new Order(_id, _name, _description);
        
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