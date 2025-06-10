using Company.TestService.Domain.Entities;

namespace Company.TestService.UnitTests.Builders;

public class ItemBuilder
{
    private Guid _id = Guid.NewGuid();
private string _title = "Test Value";
    private bool _isCompleted = true;

    public ItemBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public ItemBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ItemBuilder WithIsCompleted(bool isCompleted)
    {
        _isCompleted = isCompleted;
        return this;
    }

    public Item Build()
    {
        var entity = new Item(_title, _isCompleted);
        
        // Use reflection to set the Id if needed
        var idProperty = typeof(Item).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, _id);
        }
        
        return entity;
    }

    public static implicit operator Item(ItemBuilder builder)
    {
        return builder.Build();
    }
}