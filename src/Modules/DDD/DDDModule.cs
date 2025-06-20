using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.DDD;

public class DDDModule : ITemplateModule
{
    public string Name => "DDD";

    public bool IsEnabled(TemplateConfiguration config)
    {
        var decisions = ArchitectureRules.MakeDecisions(config);
        return decisions.EnableDDD;
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        // Create Domain project structure and .csproj file
        await CreateDomainProjectStructureAsync(context);

        if (context.Configuration.Domain?.Aggregates != null)
        {
            foreach (var aggregate in context.Configuration.Domain.Aggregates)
            {
                await GenerateAggregateAsync(context, aggregate);
                await GenerateAggregateEventsAsync(context, aggregate);
            }
        }

        if (context.Configuration.Domain?.ValueObjects != null)
        {
            foreach (var valueObject in context.Configuration.Domain.ValueObjects)
            {
                await GenerateValueObjectAsync(context, valueObject);
            }
        }

        if (context.Configuration.Domain?.Enums != null)
        {
            foreach (var enumConfig in context.Configuration.Domain.Enums)
            {
                await GenerateEnumAsync(context, enumConfig);
            }
        }
    }

    private async Task CreateDomainProjectStructureAsync(GenerationContext context)
    {
        var config = context.Configuration;

        // Generate .csproj file using relative path
        var csprojContent = GenerateDomainProjectFile(config);
        await context.WriteFileAsync($"src/Domain/{config.MicroserviceName}.Domain.csproj", csprojContent);
    }

    private string GenerateDomainProjectFile(TemplateConfiguration config)
    {
        return $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""AggregateKit"" Version=""0.2.0"" />
  </ItemGroup>

</Project>";
    }

    private async Task GenerateAggregateAsync(GenerationContext context, AggregateConfiguration aggregate)
    {
        var template = GetAggregateTemplate();
        var hasEnums = context.Configuration.Domain?.Enums?.Any() == true;
        var hasValueObjects = context.Configuration.Domain?.ValueObjects?.Any() == true;
        
        var additionalUsings = new List<string>();
        if (hasEnums)
            additionalUsings.Add($"using {context.Configuration.Namespace}.Domain.Enums;");
        if (hasValueObjects)
            additionalUsings.Add($"using {context.Configuration.Namespace}.Domain.ValueObjects;");
        
        var allUsings = string.Join("\n", additionalUsings);
        
        var content = template
            .Replace("{{Namespace}}", context.Configuration.Namespace)
            .Replace("{{AggregateName}}", aggregate.Name)
            .Replace("{{Properties}}", GenerateProperties(aggregate.Properties))
            .Replace("{{Constructor}}", GenerateConstructor(aggregate))
            .Replace("{{Methods}}", GenerateMethods(aggregate))
            .Replace("{{AdditionalUsings}}", allUsings);

        await context.WriteFileAsync($"src/Domain/Entities/{aggregate.Name}.cs", content);
    }

    private async Task GenerateAggregateEventsAsync(GenerationContext context, AggregateConfiguration aggregate)
    {
        // Generate Created event
        var createdEventTemplate = GetDomainEventTemplate();
        var createdEventContent = createdEventTemplate
            .Replace("{{Namespace}}", context.Configuration.Namespace)
            .Replace("{{EventName}}", $"{aggregate.Name}CreatedEvent")
            .Replace("{{Parameters}}", $"Guid aggregateId");

        await context.WriteFileAsync($"src/Domain/Events/{aggregate.Name}CreatedEvent.cs", createdEventContent);
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

        await context.WriteFileAsync($"src/Domain/ValueObjects/{valueObject.Name}.cs", content);
    }

    private async Task GenerateEnumAsync(GenerationContext context, EnumConfiguration enumConfig)
    {
        var template = GetEnumTemplate();
        var content = template
            .Replace("{{Namespace}}", context.Configuration.Namespace)
            .Replace("{{EnumName}}", enumConfig.Name)
            .Replace("{{EnumValues}}", GenerateEnumValues(enumConfig.Values));

        await context.WriteFileAsync($"src/Domain/Enums/{enumConfig.Name}.cs", content);
    }

    private string GenerateProperties(List<PropertyConfiguration> properties)
    {
        var customProperties = properties.Select(p => 
            $"public {p.Type} {p.Name} {{ get; private set; }}");
        
        // Add audit properties that are expected by Application layer
        var auditProperties = new[]
        {
            "public DateTime CreatedAt { get; private set; }",
            "public DateTime? UpdatedAt { get; private set; }"
        };
        
        return string.Join("\n    ", customProperties.Concat(auditProperties));
    }

    private string GenerateConstructor(AggregateConfiguration aggregate)
    {
        var parameters = string.Join(", ", aggregate.Properties.Select(p => $"{p.Type} {p.Name.ToLowerInvariant()}"));
        var assignments = string.Join("\n        ", aggregate.Properties.Select(p => $"{p.Name} = {p.Name.ToLowerInvariant()};"));
        
        return $@"public {aggregate.Name}({parameters}) : base(Guid.NewGuid())
    {{
        {assignments}
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
        
        AddDomainEvent(new {aggregate.Name}CreatedEvent(Id));
    }}";
    }

    private string GenerateMethods(AggregateConfiguration aggregate)
    {
        // Generate basic methods based on configuration
        return string.Join("\n\n    ", (aggregate.Operations ?? new List<string>()).Select(operation => 
            $"public void {operation}()\n    {{\n        // TODO: Implement {operation}\n    }}"));
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

    private string GenerateEnumValues(List<string> values)
    {
        return string.Join(",\n    ", values);
    }

    private string GenerateEventParameters(List<PropertyConfiguration> properties)
    {
        return string.Join(", ", properties.Select(p => $"{p.Type} {p.Name}"));
    }

    private string GetAggregateTemplate()
    {
        return @"using AggregateKit;
using {{Namespace}}.Domain.Events;
{{AdditionalUsings}}

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

public class {{EventName}} : DomainEventBase
{
    public Guid AggregateId { get; }
    
    public {{EventName}}({{Parameters}})
    {
        AggregateId = aggregateId;
    }
}";
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

    private string GetEnumTemplate()
    {
        return @"namespace {{Namespace}}.Domain.Enums;

public enum {{EnumName}}
{
    {{EnumValues}}
}";
    }
} 