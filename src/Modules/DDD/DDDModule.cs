using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.DDD;

public class DDDModule : ITemplateModule
{
    public string Name => "DDD";

    public bool IsEnabled(TemplateConfiguration config) => config.DDD.Enabled;

    public async Task GenerateAsync(GenerationContext context)
    {
        foreach (var aggregate in context.Configuration.DDD.Aggregates)
        {
            await GenerateAggregateAsync(context, aggregate);
            await GenerateAggregateEventsAsync(context, aggregate);
        }

        foreach (var valueObject in context.Configuration.DDD.ValueObjects)
        {
            await GenerateValueObjectAsync(context, valueObject);
        }
    }

    private async Task GenerateAggregateAsync(GenerationContext context, AggregateConfiguration aggregate)
    {
        var template = GetAggregateTemplate();
        var content = template
            .Replace("{{Namespace}}", context.Configuration.Namespace)
            .Replace("{{AggregateName}}", aggregate.Name)
            .Replace("{{Properties}}", GenerateProperties(aggregate.Properties))
            .Replace("{{Constructor}}", GenerateConstructor(aggregate))
            .Replace("{{Methods}}", GenerateMethods(aggregate));

        await context.WriteFileAsync($"src/Domain/{context.Configuration.MicroserviceName}.Domain/Entities/{aggregate.Name}.cs", content);
    }

    private async Task GenerateAggregateEventsAsync(GenerationContext context, AggregateConfiguration aggregate)
    {
        // Generate Created event
        var createdEventTemplate = GetDomainEventTemplate();
        var createdEventContent = createdEventTemplate
            .Replace("{{Namespace}}", context.Configuration.Namespace)
            .Replace("{{EventName}}", $"{aggregate.Name}CreatedEvent")
            .Replace("{{Parameters}}", $"Guid {aggregate.Name}Id, {GenerateEventParameters(aggregate.Properties)}");

        await context.WriteFileAsync($"src/Domain/{context.Configuration.MicroserviceName}.Domain/Events/{aggregate.Name}CreatedEvent.cs", createdEventContent);
    }

    private async Task GenerateValueObjectAsync(GenerationContext context, ValueObjectConfiguration valueObject)
    {
        var template = GetValueObjectTemplate();
        var content = template
            .Replace("{{Namespace}}", context.Configuration.Namespace)
            .Replace("{{ValueObjectName}}", valueObject.Name)
            .Replace("{{Properties}}", GenerateProperties(valueObject.Properties))
            .Replace("{{Constructor}}", GenerateValueObjectConstructor(valueObject))
            .Replace("{{EqualityComponents}}", GenerateEqualityComponents(valueObject.Properties));

        await context.WriteFileAsync($"src/Domain/{context.Configuration.MicroserviceName}.Domain/ValueObjects/{valueObject.Name}.cs", content);
    }

    private string GenerateProperties(List<PropertyConfiguration> properties)
    {
        return string.Join("\n    ", properties.Select(p => 
            $"public {p.Type} {p.Name} {{ get; private set; }}"));
    }

    private string GenerateConstructor(AggregateConfiguration aggregate)
    {
        var parameters = string.Join(", ", aggregate.Properties.Select(p => $"{p.Type} {p.Name.ToLowerInvariant()}"));
        var assignments = string.Join("\n        ", aggregate.Properties.Select(p => $"{p.Name} = {p.Name.ToLowerInvariant()};"));
        
        return $@"public {aggregate.Name}({parameters}) : base(Guid.NewGuid())
    {{
        {assignments}
        
        AddDomainEvent(new {aggregate.Name}CreatedEvent(Id{(aggregate.Properties.Any() ? ", " + string.Join(", ", aggregate.Properties.Select(p => p.Name)) : "")}));
    }}";
    }

    private string GenerateMethods(AggregateConfiguration aggregate)
    {
        // Generate basic methods based on configuration
        return string.Join("\n\n    ", aggregate.Methods.Select(method => 
            $"public void {method}()\n    {{\n        // TODO: Implement {method}\n    }}"));
    }

    private string GenerateValueObjectConstructor(ValueObjectConfiguration valueObject)
    {
        var parameters = string.Join(", ", valueObject.Properties.Select(p => $"{p.Type} {p.Name.ToLowerInvariant()}"));
        var assignments = string.Join("\n        ", valueObject.Properties.Select(p => $"{p.Name} = {p.Name.ToLowerInvariant()};"));
        
        return $@"public {valueObject.Name}({parameters})
    {{
        {assignments}
    }}";
    }

    private string GenerateEqualityComponents(List<PropertyConfiguration> properties)
    {
        return string.Join("\n        ", properties.Select(p => $"yield return {p.Name};"));
    }

    private string GenerateEventParameters(List<PropertyConfiguration> properties)
    {
        return string.Join(", ", properties.Select(p => $"{p.Type} {p.Name}"));
    }

    private string GetAggregateTemplate()
    {
        return @"using AggregateKit;
using {{Namespace}}.Domain.Events;

namespace {{Namespace}}.Domain.Entities;

public class {{AggregateName}} : AggregateRoot<Guid>
{
    {{Properties}}

    private {{AggregateName}}() { } // For EF Core

    {{Constructor}}

    {{Methods}}
}";
    }

    private string GetDomainEventTemplate()
    {
        return @"using AggregateKit;

namespace {{Namespace}}.Domain.Events;

public record {{EventName}}({{Parameters}}) : DomainEventBase;";
    }

    private string GetValueObjectTemplate()
    {
        return @"using AggregateKit;

namespace {{Namespace}}.Domain.ValueObjects;

public class {{ValueObjectName}} : ValueObject
{
    {{Properties}}

    {{Constructor}}

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        {{EqualityComponents}}
    }
}";
    }
} 