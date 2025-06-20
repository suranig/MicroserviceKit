using EventStoreService.Domain.Entities;

namespace EventStoreService.UnitTests.Builders;

public class EventBuilder
{
    private Guid _id = Guid.NewGuid();
private Guid _id = Guid.NewGuid();
    private string _name = "Test Value";
    private string _description = "Test Value";

    public EventBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public EventBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EventBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public EventBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public Event Build()
    {
        var entity = new Event(_id, _name, _description);
        
        // Use reflection to set the Id if needed
        var idProperty = typeof(Event).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, _id);
        }
        
        return entity;
    }

    public static implicit operator Event(EventBuilder builder)
    {
        return builder.Build();
    }
}