using TestService.Domain.Entities;

namespace TestService.UnitTests.Builders;

public class ProductBuilder
{
    private Guid _id = Guid.NewGuid();
private Guid _id = Guid.NewGuid();
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public ProductBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public ProductBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ProductBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public Product Build()
    {
        var entity = new Product(_id, _createdAt, _updatedAt);
        
        // Use reflection to set the Id if needed
        var idProperty = typeof(Product).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, _id);
        }
        
        return entity;
    }

    public static implicit operator Product(ProductBuilder builder)
    {
        return builder.Build();
    }
}