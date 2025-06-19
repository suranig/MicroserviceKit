using MicroserviceGenerator.CLI.Models;
using Microservice.Core.TemplateEngine.Configuration;
using Microservice.Core.TemplateEngine;

namespace MicroserviceGenerator.CLI.Services;

public class GenerateService
{
    private readonly TemplateService _templateService;
    private readonly ValidationService _validationService;
    
    public GenerateService()
    {
        _templateService = new TemplateService();
        _validationService = new ValidationService();
    }
    
    public async Task GenerateAsync(GenerationOptions options)
    {
        Console.WriteLine($"📋 Loading template: {options.TemplateName}");
        
        // Load template configuration
        var config = await _templateService.LoadTemplateConfigurationAsync(options.TemplateName);
        if (config == null)
        {
            throw new InvalidOperationException($"Template not found: {options.TemplateName}");
        }
        
        Console.WriteLine($"✅ Template loaded: {config.MicroserviceName}");
        
        // Apply customizations
        config = ApplyCustomizations(config, options);
        
        // Validate final configuration
        var validationResult = _validationService.ValidateTemplate(config);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Template validation failed: {string.Join(", ", validationResult.Errors)}");
        }
        
        // Show warnings if any
        if (validationResult.Warnings.Any())
        {
            Console.WriteLine("⚠️ Warnings:");
            foreach (var warning in validationResult.Warnings)
            {
                Console.WriteLine($"   • {warning}");
            }
            Console.WriteLine();
        }
        
        Console.WriteLine($"🔧 Applying customizations...");
        
        // Generate microservice using simple approach
        await GenerateMicroserviceAsync(config);
        
        Console.WriteLine($"📁 Files generated in: {Path.GetFullPath(options.OutputPath)}");
    }
    
    private TemplateConfiguration ApplyCustomizations(TemplateConfiguration config, GenerationOptions options)
    {
        // Update basic properties
        config.MicroserviceName = options.ServiceName;
        config.OutputPath = options.OutputPath;
        
        // Apply custom aggregates
        if (options.CustomAggregates.Any())
        {
            config.Domain ??= new DomainConfiguration();
            config.Domain.Aggregates = options.CustomAggregates.Select(name => 
                new AggregateConfiguration 
                { 
                    Name = name,
                    Properties = new List<PropertyConfiguration>
                    {
                        new() { Name = "Id", Type = "Guid", IsRequired = true },
                        new() { Name = "CreatedAt", Type = "DateTime", IsRequired = true },
                        new() { Name = "UpdatedAt", Type = "DateTime", IsRequired = false }
                    },
                    Operations = new List<string> { "Create", "Update", "Delete" }
                }).ToList();
                
            Console.WriteLine($"   ✅ Custom aggregates: {string.Join(", ", options.CustomAggregates)}");
        }
        
        // Apply external services
        if (options.ExternalServices.Any())
        {
            config.Features ??= new FeaturesConfiguration();
            config.Features.ExternalServices ??= new ExternalServicesConfiguration();
            config.Features.ExternalServices.Enabled = true;
            config.Features.ExternalServices.Services = options.ExternalServices.Select(name =>
                new ExternalServiceConfiguration 
                { 
                    Name = name,
                    BaseUrl = $"https://api.{name.ToLowerInvariant()}.com",
                    Type = "http",
                    Operations = new List<string> { "Get", "Create", "Update", "Delete" }
                }).ToList();
                
            Console.WriteLine($"   ✅ External services: {string.Join(", ", options.ExternalServices)}");
        }
        
        // Apply database provider
        if (!string.IsNullOrEmpty(options.DatabaseProvider))
        {
            config.Features ??= new FeaturesConfiguration();
            config.Features.Database ??= new DatabaseConfiguration();
            config.Features.Database.WriteModel ??= new WriteModelConfiguration();
            config.Features.Database.WriteModel.Provider = options.DatabaseProvider;
            
            Console.WriteLine($"   ✅ Database provider: {options.DatabaseProvider}");
        }
        
        // Apply messaging provider
        if (!string.IsNullOrEmpty(options.MessagingProvider))
        {
            config.Features ??= new FeaturesConfiguration();
            config.Features.Messaging ??= new MessagingConfiguration();
            config.Features.Messaging.Enabled = true;
            config.Features.Messaging.Provider = options.MessagingProvider;
            
            Console.WriteLine($"   ✅ Messaging provider: {options.MessagingProvider}");
        }
        
        // Apply authentication type
        if (!string.IsNullOrEmpty(options.AuthenticationType))
        {
            config.Features ??= new FeaturesConfiguration();
            config.Features.Api ??= new ApiConfiguration();
            config.Features.Api.Authentication = options.AuthenticationType;
            
            Console.WriteLine($"   ✅ Authentication: {options.AuthenticationType}");
        }
        
        // Apply API style
        if (!string.IsNullOrEmpty(options.ApiStyle))
        {
            config.Features ??= new FeaturesConfiguration();
            config.Features.Api ??= new ApiConfiguration();
            config.Features.Api.Style = options.ApiStyle;
            
            Console.WriteLine($"   ✅ API style: {options.ApiStyle}");
        }
        
        return config;
    }
    
    private async Task GenerateMicroserviceAsync(TemplateConfiguration config)
    {
        // Create output directory
        Directory.CreateDirectory(config.OutputPath);
        
        // Generate basic project structure
        await GenerateProjectStructureAsync(config);
        
        // Generate solution file
        await GenerateSolutionFileAsync(config);
        
        // Generate README
        await GenerateReadmeAsync(config);
        
        Console.WriteLine($"✅ Basic project structure generated");
    }
    
    private async Task GenerateProjectStructureAsync(TemplateConfiguration config)
    {
        var srcPath = Path.Combine(config.OutputPath, "src");
        Directory.CreateDirectory(srcPath);
        
        // Create basic directories
        Directory.CreateDirectory(Path.Combine(srcPath, "Domain"));
        Directory.CreateDirectory(Path.Combine(srcPath, "Application"));
        Directory.CreateDirectory(Path.Combine(srcPath, "Infrastructure"));
        Directory.CreateDirectory(Path.Combine(srcPath, "Api"));
        
        var testsPath = Path.Combine(config.OutputPath, "tests");
        Directory.CreateDirectory(testsPath);
        
        Console.WriteLine($"   📁 Created project directories");
    }
    
    private async Task GenerateSolutionFileAsync(TemplateConfiguration config)
    {
        var solutionContent = $@"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1

Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{config.MicroserviceName}.Domain"", ""src\Domain\{config.MicroserviceName}.Domain.csproj"", ""{{5C9F7570-3036-466E-B4EF-3307486F3391}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{config.MicroserviceName}.Application"", ""src\Application\{config.MicroserviceName}.Application.csproj"", ""{{F736B777-1905-48BC-9DF0-CB561A7BF9D2}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{config.MicroserviceName}.Infrastructure"", ""src\Infrastructure\{config.MicroserviceName}.Infrastructure.csproj"", ""{{536A1C0B-964A-4830-A8F1-1B296CD5E2D3}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{config.MicroserviceName}.Api"", ""src\Api\{config.MicroserviceName}.Api.csproj"", ""{{373FE1FF-A402-4860-83F9-CA5E902468E4}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{config.MicroserviceName}.Tests"", ""tests\{config.MicroserviceName}.Tests.csproj"", ""{{8B2A1C0B-964A-4830-A8F1-1B296CD5E2D5}}""
EndProject

Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{{5C9F7570-3036-466E-B4EF-3307486F3391}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{5C9F7570-3036-466E-B4EF-3307486F3391}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{5C9F7570-3036-466E-B4EF-3307486F3391}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{5C9F7570-3036-466E-B4EF-3307486F3391}}.Release|Any CPU.Build.0 = Release|Any CPU
		{{F736B777-1905-48BC-9DF0-CB561A7BF9D2}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{F736B777-1905-48BC-9DF0-CB561A7BF9D2}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{F736B777-1905-48BC-9DF0-CB561A7BF9D2}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{F736B777-1905-48BC-9DF0-CB561A7BF9D2}}.Release|Any CPU.Build.0 = Release|Any CPU
		{{536A1C0B-964A-4830-A8F1-1B296CD5E2D3}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{536A1C0B-964A-4830-A8F1-1B296CD5E2D3}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{536A1C0B-964A-4830-A8F1-1B296CD5E2D3}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{536A1C0B-964A-4830-A8F1-1B296CD5E2D3}}.Release|Any CPU.Build.0 = Release|Any CPU
		{{373FE1FF-A402-4860-83F9-CA5E902468E4}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{373FE1FF-A402-4860-83F9-CA5E902468E4}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{373FE1FF-A402-4860-83F9-CA5E902468E4}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{373FE1FF-A402-4860-83F9-CA5E902468E4}}.Release|Any CPU.Build.0 = Release|Any CPU
		{{8B2A1C0B-964A-4830-A8F1-1B296CD5E2D5}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{8B2A1C0B-964A-4830-A8F1-1B296CD5E2D5}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{8B2A1C0B-964A-4830-A8F1-1B296CD5E2D5}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{8B2A1C0B-964A-4830-A8F1-1B296CD5E2D5}}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal";

        var solutionPath = Path.Combine(config.OutputPath, $"{config.MicroserviceName}.sln");
        await File.WriteAllTextAsync(solutionPath, solutionContent);
        
        Console.WriteLine($"   📄 Generated solution file: {config.MicroserviceName}.sln");
    }
    
    private async Task GenerateReadmeAsync(TemplateConfiguration config)
    {
        var readmeContent = $@"# {config.MicroserviceName}

Generated microservice using MicroserviceKit.

## Architecture

This microservice implements:
- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS patterns
- Event-driven architecture

## Getting Started

### Prerequisites
- .NET 8.0 SDK

### Running locally
```bash
dotnet restore
dotnet build
dotnet run --project src/Api/{config.MicroserviceName}.Api
```

## API Documentation

The API will be available at: http://localhost:5000/swagger

## Generated Features

{(config.Domain?.Aggregates?.Any() == true ? 
    $"### Aggregates\n{string.Join("\n", config.Domain.Aggregates.Select(a => $"- **{a.Name}**: {string.Join(", ", a.Properties.Select(p => $"{p.Name} ({p.Type})"))}"))} " : 
    "No aggregates defined")}

{(config.Features?.ExternalServices?.Services?.Any() == true ? 
    $"### External Services\n{string.Join("\n", config.Features.ExternalServices.Services.Select(s => $"- **{s.Name}**: {s.BaseUrl}"))}" : 
    "")}

## Configuration

Generated with:
- Database: {config.GetDatabaseProvider()}
- Read Model: {config.GetReadModelProvider()}
- Architecture Level: {config.Architecture?.Level ?? "standard"}
";

        var readmePath = Path.Combine(config.OutputPath, "README.md");
        await File.WriteAllTextAsync(readmePath, readmeContent);
        
        Console.WriteLine($"   📄 Generated README.md");
    }
} 