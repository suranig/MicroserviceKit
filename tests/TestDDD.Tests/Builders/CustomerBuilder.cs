using ECommerce.OrderService.Domain.Entities;

namespace ECommerce.OrderService.UnitTests.Builders;

public class CustomerBuilder
{
    private Guid _id = Guid.NewGuid();
private string _email = "Test Value";
    private string _name = "Test Value";

    public CustomerBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CustomerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Customer Build()
    {
        var entity = new Customer(_email, _name);
        
        // Use reflection to set the Id if needed
        var idProperty = typeof(Customer).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, _id);
        }
        
        return entity;
    }

    public static implicit operator Customer(CustomerBuilder builder)
    {
        return builder.Build();
    }
}