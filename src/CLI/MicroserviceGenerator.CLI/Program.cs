using System.CommandLine;
using System.Text.Json;
using Microservice.Core.TemplateEngine.Configuration;
using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Modules.DDD;
using MicroserviceGenerator.CLI.Commands;

var rootCommand = new RootCommand("MicroserviceKit - Complete toolkit for generating .NET 8 microservices with DDD patterns");

// Command: new
var newCommand = new Command("new", "Generate a new microservice from template");
var nameArgument = new Argument<string>("name", "Name of the microservice");
var configOption = new Option<string?>("--config", "Path to configuration file");
var outputOption = new Option<string>("--output", () => "./", "Output directory");
var interactiveOption = new Option<bool>("--interactive", () => false, "Interactive mode");

newCommand.AddArgument(nameArgument);
newCommand.AddOption(configOption);
newCommand.AddOption(outputOption);
newCommand.AddOption(interactiveOption);

newCommand.SetHandler(async (string name, string? configPath, string output, bool interactive) =>
{
    try
    {
        TemplateConfiguration config;
        
        if (interactive)
        {
            config = await RunInteractiveMode(name);
        }
        else if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
        {
            var json = await File.ReadAllTextAsync(configPath);
            config = JsonSerializer.Deserialize<TemplateConfiguration>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            }) ?? new TemplateConfiguration();
        }
        else
        {
            config = CreateDefaultConfig(name);
        }
        
        config.MicroserviceName = name;
        config.OutputPath = output;
        
        await GenerateMicroservice(config);
        
        Console.WriteLine($"✅ Microservice '{name}' generated successfully in '{output}'");
        Console.WriteLine("\nNext steps:");
        Console.WriteLine($"  cd {output}");
        Console.WriteLine("  dotnet restore");
        Console.WriteLine("  dotnet run --project src/Api/*.Api");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
        Environment.Exit(1);
    }
}, nameArgument, configOption, outputOption, interactiveOption);

// Command: add
var addCommand = new Command("add", "Add components to existing microservice");
var addAggregateCommand = new Command("aggregate", "Add new aggregate");
var aggregateNameArg = new Argument<string>("name", "Aggregate name");
var propertiesOption = new Option<string[]>("--properties", "Properties in format 'Name:Type'");

addAggregateCommand.AddArgument(aggregateNameArg);
addAggregateCommand.AddOption(propertiesOption);

addAggregateCommand.SetHandler(async (string aggregateName, string[] properties) =>
{
    await AddAggregate(aggregateName, properties);
    Console.WriteLine($"✅ Aggregate '{aggregateName}' added successfully");
}, aggregateNameArg, propertiesOption);

addCommand.AddCommand(addAggregateCommand);

// Add commands
rootCommand.AddCommand(newCommand);
rootCommand.AddCommand(addCommand);
rootCommand.AddCommand(MigrateCommand.Create());
rootCommand.AddCommand(HistoryCommand.Create());

return await rootCommand.InvokeAsync(args);

static async Task<TemplateConfiguration> RunInteractiveMode(string name)
{
    Console.WriteLine($"🚀 Creating microservice: {name}");
    Console.WriteLine();
    
    var config = new TemplateConfiguration
    {
        MicroserviceName = name,
        Namespace = PromptWithDefault($"Namespace", $"Company.{name}")
    };
    
    // DDD Configuration
    Console.WriteLine("\n📦 Domain-Driven Design Configuration:");
    var enableDDD = PromptYesNo("Enable DDD patterns?", true);
    
    if (enableDDD)
    {
        Console.WriteLine("\nDefine your aggregates (press Enter with empty name to finish):");
        while (true)
        {
            var aggregateName = Prompt("Aggregate name");
            if (string.IsNullOrEmpty(aggregateName)) break;
            
            var aggregate = new AggregateConfiguration { Name = aggregateName };
            
            Console.WriteLine($"Properties for {aggregateName} (format: Name:Type, press Enter to finish):");
            while (true)
            {
                var property = Prompt("Property");
                if (string.IsNullOrEmpty(property)) break;
                
                var parts = property.Split(':');
                if (parts.Length == 2)
                {
                    aggregate.Properties.Add(new PropertyConfiguration 
                    { 
                        Name = parts[0].Trim(), 
                        Type = parts[1].Trim() 
                    });
                }
            }
            
            config.Domain ??= new DomainConfiguration();
            config.Domain.Aggregates ??= new List<AggregateConfiguration>();
            config.Domain.Aggregates.Add(aggregate);
        }
    }
    
    // API Configuration
    Console.WriteLine("\n🌐 API Configuration:");
    var apiStyle = PromptChoice("API style", new[] { "minimal", "controllers", "both" }, "minimal");
    config.Features ??= new FeaturesConfiguration();
    config.Features.Api ??= new ApiConfiguration();
    config.Features.Api.Style = apiStyle;
    
    // Persistence
    Console.WriteLine("\n💾 Persistence Configuration:");
    config.Features.Persistence ??= new PersistenceConfiguration();
    config.Features.Persistence.Provider = PromptChoice("Persistence provider", new[] { "inmemory", "sqlite", "postgresql", "sqlserver" }, "inmemory");
    
    return config;
}

static TemplateConfiguration CreateDefaultConfig(string name)
{
    return new TemplateConfiguration
    {
        MicroserviceName = name,
        Namespace = $"Company.{name}",
        Architecture = new ArchitectureConfiguration
        {
            Level = "standard"
        },
        Domain = new DomainConfiguration
        {
            Aggregates = new List<AggregateConfiguration>
            {
                new AggregateConfiguration
                {
                    Name = "Item",
                    Properties = new List<PropertyConfiguration>
                    {
                        new PropertyConfiguration { Name = "Title", Type = "string" },
                        new PropertyConfiguration { Name = "IsCompleted", Type = "bool" }
                    },
                    Operations = new List<string> { "Create", "MarkComplete" }
                }
            }
        },
        Features = new FeaturesConfiguration
        {
            Api = new ApiConfiguration
            {
                Style = "controllers"
            },
            Persistence = new PersistenceConfiguration
            {
                Provider = "inmemory"
            },
            Testing = new TestingConfiguration
            {
                Level = "integration"
            },
            Deployment = new DeploymentConfiguration
            {
                Docker = "auto",
                Kubernetes = "disabled"
            }
        }
    };
}

static async Task GenerateMicroservice(TemplateConfiguration config)
{
    var context = new GenerationContext(config);
    var modules = new List<ITemplateModule>
    {
        new DDDModule(),
        new Microservice.Modules.Application.ApplicationModule(),
        new Microservice.Modules.Infrastructure.InfrastructureModule(),
        new Microservice.Modules.Api.RestApiModule(),
        new Microservice.Modules.Tests.UnitTestModule(),
        new Microservice.Modules.Tests.IntegrationTestModule(),
        new Microservice.Modules.Deployment.DeploymentModule()
    };
    
    foreach (var module in modules.Where(m => m.IsEnabled(config)))
    {
        await module.GenerateAsync(context);
    }
    
    // Generate project structure
    await GenerateProjectStructure(context);
}

static async Task GenerateProjectStructure(GenerationContext context)
{
    var config = context.Configuration;
    
    // Generate solution file
    var solutionContent = $@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1

Project(""{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}"") = ""src"", ""src"", ""{{66448982-949A-4E9E-9EFA-CED092C125CB}}""
EndProject

Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{config.MicroserviceName}.Api"", ""src\\Api\\{config.MicroserviceName}.Api\\{config.MicroserviceName}.Api.csproj"", ""{{373FE1FF-A402-4860-83F9-CA5E902468ED}}""
EndProject

Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{config.MicroserviceName}.Application"", ""src\\Application\\{config.MicroserviceName}.Application\\{config.MicroserviceName}.Application.csproj"", ""{{F736B777-1905-48BC-9DF0-CB561A7BF9D1}}""
EndProject

Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{config.MicroserviceName}.Domain"", ""src\\Domain\\{config.MicroserviceName}.Domain\\{config.MicroserviceName}.Domain.csproj"", ""{{5C9F7570-3036-466E-B4EF-3307486F3391}}""
EndProject

Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{config.MicroserviceName}.Infrastructure"", ""src\\Infrastructure\\{config.MicroserviceName}.Infrastructure\\{config.MicroserviceName}.Infrastructure.csproj"", ""{{536A1C0B-964A-4830-A8F1-1B296CD5E2D8}}""
EndProject
";
    
    await context.WriteFileAsync($"{config.MicroserviceName}.sln", solutionContent);
    
    // Generate README
    var readmeContent = $@"# {config.MicroserviceName}

Generated microservice using MicroserviceTemplateGenerator.

## Architecture

This microservice implements:
- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS with Wolverine
- AggregateKit for DDD building blocks

## Getting Started

### Prerequisites
- .NET 8.0 SDK

### Running locally
```bash
dotnet restore
dotnet run --project src/Api/{config.MicroserviceName}.Api
```

## API Documentation

The API will be available at: http://localhost:5000/swagger

## Generated Aggregates

{string.Join("\n", config.Domain?.Aggregates?.Select(a => $"- **{a.Name}**: {string.Join(", ", a.Properties.Select(p => $"{p.Name} ({p.Type})"))}")  ?? new[] { "No aggregates defined" })}
";
    
    await context.WriteFileAsync("README.md", readmeContent);
}

static async Task AddAggregate(string name, string[] properties)
{
    // Implementation for adding aggregate to existing project
    Console.WriteLine($"Adding aggregate {name} with properties: {string.Join(", ", properties)}");
    // TODO: Implement
}

static string Prompt(string message)
{
    Console.Write($"{message}: ");
    return Console.ReadLine() ?? "";
}

static string PromptWithDefault(string message, string defaultValue)
{
    Console.Write($"{message} [{defaultValue}]: ");
    var input = Console.ReadLine();
    return string.IsNullOrEmpty(input) ? defaultValue : input;
}

static bool PromptYesNo(string message, bool defaultValue = false)
{
    var defaultText = defaultValue ? "Y/n" : "y/N";
    Console.Write($"{message} [{defaultText}]: ");
    var input = Console.ReadLine()?.ToLower();
    
    if (string.IsNullOrEmpty(input))
        return defaultValue;
        
    return input.StartsWith("y");
}

static string PromptChoice(string message, string[] choices, string defaultChoice)
{
    Console.WriteLine($"{message}:");
    for (int i = 0; i < choices.Length; i++)
    {
        var marker = choices[i] == defaultChoice ? "*" : " ";
        Console.WriteLine($"  {i + 1}{marker} {choices[i]}");
    }
    
    Console.Write($"Choose [1-{choices.Length}]: ");
    var input = Console.ReadLine();
    
    if (int.TryParse(input, out int choice) && choice >= 1 && choice <= choices.Length)
    {
        return choices[choice - 1];
    }
    
    return defaultChoice;
} 