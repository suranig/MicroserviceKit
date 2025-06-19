using System.CommandLine;
using System.Text;
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
    
    // Architecture Level
    Console.WriteLine("\n🏗️ Architecture Configuration:");
    var level = PromptChoice("Architecture level", new[] { "minimal", "standard", "enterprise" }, "standard");
    config.Architecture = new ArchitectureConfiguration { Level = level };
    
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
    var apiStyle = PromptChoice("API style", new[] { "minimal", "controllers", "both" }, "controllers");
    var authType = PromptChoice("Authentication", new[] { "none", "jwt", "oauth" }, "none");
    
    config.Features ??= new FeaturesConfiguration();
    config.Features.Api ??= new ApiConfiguration();
    config.Features.Api.Style = apiStyle;
    config.Features.Api.Authentication = authType;
    
    // Database Configuration
    Console.WriteLine("\n💾 Database Configuration:");
    var enableDatabase = PromptYesNo("Configure database?", true);
    
    if (enableDatabase)
    {
        config.Features.Database ??= new DatabaseConfiguration();
        
        // Write Model (Primary Database)
        Console.WriteLine("\n📝 Write Model Database:");
        var writeProvider = PromptChoice("Write database provider", 
            new[] { "postgresql", "mysql", "sqlserver", "sqlite" }, "postgresql");
        
        config.Features.Database.WriteModel = new WriteModelConfiguration
        {
            Provider = writeProvider,
            EnableMigrations = PromptYesNo("Enable migrations?", true),
            EnableAuditing = PromptYesNo("Enable auditing?", false),
            EnableSoftDelete = PromptYesNo("Enable soft delete?", false)
        };
        
        // Read Model Configuration
        Console.WriteLine("\n📖 Read Model Configuration:");
        var enableReadModel = PromptYesNo("Use separate read model?", false);
        
        if (enableReadModel)
        {
            var readProvider = PromptChoice("Read model provider", 
                new[] { "mongodb", "elasticsearch", "same" }, "mongodb");
            
            config.Features.Database.ReadModel = new ReadModelConfiguration
            {
                Provider = readProvider,
                EnableProjections = PromptYesNo("Enable projections?", true),
                SyncStrategy = PromptChoice("Sync strategy", 
                    new[] { "eventual", "immediate", "batch" }, "eventual")
            };
        }
        
        // Cache Configuration
        Console.WriteLine("\n🗄️ Cache Configuration:");
        var enableCache = PromptYesNo("Enable caching?", false);
        
        if (enableCache)
        {
            var cacheProvider = PromptChoice("Cache provider", 
                new[] { "redis", "inmemory", "distributed" }, "redis");
            
            config.Features.Database.Cache = new CacheConfiguration
            {
                Enabled = true,
                Provider = cacheProvider,
                DefaultTtlMinutes = int.Parse(PromptWithDefault("Default TTL (minutes)", "60"))
            };
        }
    }
    
    // Messaging Configuration
    Console.WriteLine("\n📨 Messaging Configuration:");
    var enableMessaging = PromptYesNo("Enable messaging (events)?", false);
    
    if (enableMessaging)
    {
        var messagingProvider = PromptChoice("Messaging provider", 
            new[] { "rabbitmq", "servicebus", "inmemory" }, "rabbitmq");
        
        config.Features.Messaging = new MessagingConfiguration
        {
            Enabled = true,
            Provider = messagingProvider,
            Patterns = new List<string>()
        };
        
        if (PromptYesNo("Enable outbox pattern?", true))
            config.Features.Messaging.Patterns.Add("outbox");
        
        if (PromptYesNo("Enable domain events?", true))
            config.Features.Messaging.Patterns.Add("events");
        
        if (PromptYesNo("Enable saga pattern?", false))
            config.Features.Messaging.Patterns.Add("saga");
    }
    
    // External Services Configuration
    Console.WriteLine("\n🔗 External Services Configuration:");
    var enableExternalServices = PromptYesNo("Configure external services?", false);
    
    if (enableExternalServices)
    {
        config.Features.ExternalServices = new ExternalServicesConfiguration
        {
            Enabled = true,
            Services = new List<ExternalServiceConfiguration>(),
            Resilience = new ResilienceConfiguration()
        };
        
        Console.WriteLine("\nDefine external services (press Enter with empty name to finish):");
        while (true)
        {
            var serviceName = Prompt("Service name");
            if (string.IsNullOrEmpty(serviceName)) break;
            
            var baseUrl = Prompt($"Base URL for {serviceName}");
            var serviceType = PromptChoice("Service type", 
                new[] { "http", "grpc", "graphql" }, "http");
            
            var externalService = new ExternalServiceConfiguration
            {
                Name = serviceName,
                BaseUrl = baseUrl,
                Type = serviceType,
                Operations = new List<string>()
            };
            
            Console.WriteLine($"Operations for {serviceName} (press Enter to finish):");
            while (true)
            {
                var operation = Prompt("Operation name");
                if (string.IsNullOrEmpty(operation)) break;
                externalService.Operations.Add(operation);
            }
            
            config.Features.ExternalServices.Services.Add(externalService);
        }
        
        // Resilience Configuration
        Console.WriteLine("\n🛡️ Resilience Configuration:");
        config.Features.ExternalServices.Resilience.Retry.Enabled = PromptYesNo("Enable retry policy?", true);
        if (config.Features.ExternalServices.Resilience.Retry.Enabled)
        {
            config.Features.ExternalServices.Resilience.Retry.MaxAttempts = 
                int.Parse(PromptWithDefault("Max retry attempts", "3"));
        }
        
        config.Features.ExternalServices.Resilience.CircuitBreaker.Enabled = 
            PromptYesNo("Enable circuit breaker?", true);
        
        config.Features.ExternalServices.Resilience.Timeout.Enabled = 
            PromptYesNo("Enable timeout policy?", true);
    }
    
    // Testing Configuration
    Console.WriteLine("\n🧪 Testing Configuration:");
    var testLevel = PromptChoice("Testing level", 
        new[] { "unit", "integration", "full", "enterprise" }, "integration");
    
    config.Features.Testing = new TestingConfiguration
    {
        Level = testLevel,
        MockingEnabled = PromptYesNo("Enable mocking?", true),
        TestContainersEnabled = PromptYesNo("Enable TestContainers?", testLevel == "integration" || testLevel == "full")
    };
    
    // Deployment Configuration
    Console.WriteLine("\n🚀 Deployment Configuration:");
    var enableDocker = PromptYesNo("Generate Docker configuration?", true);
    
    config.Features.Deployment = new DeploymentConfiguration
    {
        Docker = enableDocker ? "enabled" : "disabled",
        HealthChecks = "auto"
    };
    
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
                HealthChecks = "auto"
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
        new Microservice.Modules.ExternalServices.ExternalServicesModule(),
        new Microservice.Modules.Messaging.MessagingModule(),
        new Microservice.Modules.ReadModels.ReadModelsModule(),
        new Microservice.Modules.Tests.UnitTestModule(),
        new Microservice.Modules.Tests.IntegrationTestModule()
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
    var structure = config.ProjectStructure ?? new ProjectStructureConfiguration();
    var sourceDir = structure.SourceDirectory;
    var decisions = ArchitectureRules.MakeDecisions(config);
    
    // Build solution content dynamically based on generated projects
    var solutionBuilder = new StringBuilder();
    solutionBuilder.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
    solutionBuilder.AppendLine("# Visual Studio Version 17");
    solutionBuilder.AppendLine("VisualStudioVersion = 17.0.31903.59");
    solutionBuilder.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
    solutionBuilder.AppendLine();
    
    // Add source folder
    solutionBuilder.AppendLine($"Project(\"{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}\") = \"{sourceDir}\", \"{sourceDir}\", \"{{66448982-949A-4E9E-9EFA-CED092C125CB}}\"");
    solutionBuilder.AppendLine("EndProject");
    solutionBuilder.AppendLine();
    
    // Add projects based on enabled modules
    var projectId = 1;
    
    // Domain project (always present if DDD is enabled)
    if (decisions.EnableDDD)
    {
        var domainPath = context.GetDomainProjectPath();
        solutionBuilder.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{config.MicroserviceName}.Domain\", \"{domainPath}/{config.MicroserviceName}.Domain.csproj\", \"{{5C9F7570-3036-466E-B4EF-3307486F339{projectId++}}}\"");
        solutionBuilder.AppendLine("EndProject");
        solutionBuilder.AppendLine();
    }
    
    // Application project (if CQRS enabled and not minimal)
    if (decisions.EnableCQRS && decisions.ArchitectureLevel != ArchitectureLevel.Minimal)
    {
        var applicationPath = context.GetApplicationProjectPath();
        solutionBuilder.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{config.MicroserviceName}.Application\", \"{applicationPath}/{config.MicroserviceName}.Application.csproj\", \"{{F736B777-1905-48BC-9DF0-CB561A7BF9D{projectId++}}}\"");
        solutionBuilder.AppendLine("EndProject");
        solutionBuilder.AppendLine();
    }
    
    // Infrastructure project (if enabled)
    if (decisions.EnableInfrastructure)
    {
        var infrastructurePath = context.GetInfrastructureProjectPath();
        solutionBuilder.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{config.MicroserviceName}.Infrastructure\", \"{infrastructurePath}/{config.MicroserviceName}.Infrastructure.csproj\", \"{{536A1C0B-964A-4830-A8F1-1B296CD5E2D{projectId++}}}\"");
        solutionBuilder.AppendLine("EndProject");
        solutionBuilder.AppendLine();
    }
    
    // API project (always present)
    var apiPath = context.GetApiProjectPath();
    solutionBuilder.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{config.MicroserviceName}.Api\", \"{apiPath}/{config.MicroserviceName}.Api.csproj\", \"{{373FE1FF-A402-4860-83F9-CA5E902468E{projectId++}}}\"");
    solutionBuilder.AppendLine("EndProject");
    solutionBuilder.AppendLine();
    
    // Test projects
    var testsPath = context.GetTestsProjectPath();
    solutionBuilder.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{config.MicroserviceName}.Tests\", \"{testsPath}/{config.MicroserviceName}.Tests.csproj\", \"{{8B2A1C0B-964A-4830-A8F1-1B296CD5E2D{projectId++}}}\"");
    solutionBuilder.AppendLine("EndProject");
    solutionBuilder.AppendLine();
    
    var integrationTestsPath = context.GetIntegrationTestsProjectPath();
    if (config.Features?.Testing?.Level == "integration" || config.Features?.Testing?.Level == "full" || config.Features?.Testing?.Level == "enterprise")
    {
        solutionBuilder.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{config.MicroserviceName}.IntegrationTests\", \"{integrationTestsPath}/{config.MicroserviceName}.IntegrationTests.csproj\", \"{{9C3B2D1E-075F-5941-B9G2-2C307BE6F3E{projectId++}}}\"");
        solutionBuilder.AppendLine("EndProject");
        solutionBuilder.AppendLine();
    }
    
    // Add Global section
    solutionBuilder.AppendLine("Global");
    solutionBuilder.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
    solutionBuilder.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
    solutionBuilder.AppendLine("\t\tRelease|Any CPU = Release|Any CPU");
    solutionBuilder.AppendLine("\tEndGlobalSection");
    solutionBuilder.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
    
    // Add project configurations for each project
    var projectIds = new[]
    {
        "{5C9F7570-3036-466E-B4EF-3307486F3391}", // Domain
        "{373FE1FF-A402-4860-83F9-CA5E902468E2}", // Api  
        "{8B2A1C0B-964A-4830-A8F1-1B296CD5E2D3}" // Tests
    };
    
    foreach (var id in projectIds)
    {
        solutionBuilder.AppendLine($"\t\t{id}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
        solutionBuilder.AppendLine($"\t\t{id}.Debug|Any CPU.Build.0 = Debug|Any CPU");
        solutionBuilder.AppendLine($"\t\t{id}.Release|Any CPU.ActiveCfg = Release|Any CPU");
        solutionBuilder.AppendLine($"\t\t{id}.Release|Any CPU.Build.0 = Release|Any CPU");
    }
    
    solutionBuilder.AppendLine("\tEndGlobalSection");
    solutionBuilder.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
    solutionBuilder.AppendLine("\t\tHideSolutionNode = FALSE");
    solutionBuilder.AppendLine("\tEndGlobalSection");
    solutionBuilder.AppendLine("EndGlobal");
    
    var solutionContent = solutionBuilder.ToString();
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
dotnet run --project {sourceDir}/Api/{config.MicroserviceName}.Api
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