using ReadModelService.Domain.Entities;

namespace ReadModelService.UnitTests.Builders;

public class ProductBuilder
{
    private Guid _id = Guid.NewGuid();
private Guid _id = Guid.NewGuid();
    private string _name = "Test Value";
    private string _description = "Test Value";

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

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public Product Build()
    {
        var entity = new Product(_id, _name, _description);
        
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