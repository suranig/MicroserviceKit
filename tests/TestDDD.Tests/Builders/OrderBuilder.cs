using ECommerce.OrderService.Domain.Entities;

namespace ECommerce.OrderService.UnitTests.Builders;

public class OrderBuilder
{
    private Guid _id = Guid.NewGuid();
private Guid _customerId = Guid.NewGuid();
    private decimal _totalAmount = 123.45m;
    private string _status = "Test Value";

    public OrderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public OrderBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder WithTotalAmount(decimal totalAmount)
    {
        _totalAmount = totalAmount;
        return this;
    }

    public OrderBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public Order Build()
    {
        var entity = new Order(_customerId, _totalAmount, _status);
        
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