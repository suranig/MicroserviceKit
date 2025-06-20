using SimpleService.Domain.Entities;

namespace SimpleService.UnitTests.Builders;

public class UserBuilder
{
    private Guid _id = Guid.NewGuid();
private Guid _id = Guid.NewGuid();
    private string _name = "Test Value";
    private string _description = "Test Value";

    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UserBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public User Build()
    {
        var entity = new User(_id, _name, _description);
        
        // Use reflection to set the Id if needed
        var idProperty = typeof(User).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, _id);
        }
        
        return entity;
    }

    public static implicit operator User(UserBuilder builder)
    {
        return builder.Build();
    }
}