using WorkflowService.Domain.Entities;

namespace WorkflowService.UnitTests.Builders;

public class WorkflowBuilder
{
    private Guid _id = Guid.NewGuid();
private Guid _id = Guid.NewGuid();
    private string _name = "Test Value";
    private string _description = "Test Value";

    public WorkflowBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

public WorkflowBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public WorkflowBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public WorkflowBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public Workflow Build()
    {
        var entity = new Workflow(_id, _name, _description);
        
        // Use reflection to set the Id if needed
        var idProperty = typeof(Workflow).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(entity, _id);
        }
        
        return entity;
    }

    public static implicit operator Workflow(WorkflowBuilder builder)
    {
        return builder.Build();
    }
}