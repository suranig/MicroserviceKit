using MyApp.UserService.Domain.Entities;

namespace MyApp.UserService.UnitTests.Builders;

public class UserBuilder
{
    private Guid _id = Guid.NewGuid();
private string _email = "Test Value";
    private string _name = "Test Value";

    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public User Build()
    {
        var entity = new User(_email, _name);
        
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