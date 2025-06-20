using ReadModelService.Domain.Entities;

namespace ReadModelService.UnitTests.Builders;

public class PageBuilder
{
    private Guid _id = Guid.NewGuid();
private Guid _id = Guid.NewGuid();
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _updatedAt = DateTime.UtcNow;

    public PageBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public PageBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PageBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public PageBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public Page Build()
    {
        var entity = new Page(_id, _createdAt, _updatedAt);
        
        // Use reflection to set the Id if needed
        var idProperty = typeof(Page).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, _id);
        }
        
        return entity;
    }

    public static implicit operator Page(PageBuilder builder)
    {
        return builder.Build();
    }
}