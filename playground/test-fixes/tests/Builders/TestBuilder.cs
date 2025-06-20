using TestService.Domain.Entities;

namespace TestService.UnitTests.Builders;

public class TestBuilder
{
    private Guid _id = Guid.NewGuid();
private Guid _id = Guid.NewGuid();
    private string _name = "Test Value";
    private string _description = "Test Value";
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public TestBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public TestBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public TestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public TestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TestBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public TestBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public Test Build()
    {
        var entity = new Test(_id, _name, _description, _createdAt, _updatedAt);
        
        // Use reflection to set the Id if needed
        var idProperty = typeof(Test).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, _id);
        }
        
        return entity;
    }

    public static implicit operator Test(TestBuilder builder)
    {
        return builder.Build();
    }
}