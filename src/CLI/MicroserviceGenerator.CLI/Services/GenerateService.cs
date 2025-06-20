using MicroserviceGenerator.CLI.Models;
using Microservice.Core.TemplateEngine.Configuration;
using Microservice.Core.TemplateEngine;
using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceGenerator.CLI.Services;

public class GenerateService
{
    private readonly TemplateService _templateService;
    private readonly ValidationService _validationService;
    private readonly IServiceProvider _serviceProvider;
    
    public GenerateService()
    {
        _templateService = new TemplateService();
        _validationService = new ValidationService();
        _serviceProvider = ConfigureServices();
    }
    
    private IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add Template Engine
        services.AddSingleton<ITemplateEngine, Microservice.Core.TemplateEngine.TemplateEngine>();
        
        // Register all modules directly by type
        services.AddSingleton<ITemplateModule, Microservice.Modules.DDD.DDDModule>();
        services.AddSingleton<ITemplateModule, Microservice.Modules.Application.ApplicationModule>();
        services.AddSingleton<ITemplateModule, Microservice.Modules.Infrastructure.InfrastructureModule>();
        services.AddSingleton<ITemplateModule, Microservice.Modules.Api.ApiModule>();
        services.AddSingleton<ITemplateModule, Microservice.Modules.Api.RestApiModule>();
        services.AddSingleton<ITemplateModule, Microservice.Modules.ExternalServices.ExternalServicesModule>();
        services.AddSingleton<ITemplateModule, Microservice.Modules.Messaging.MessagingModule>();
        services.AddSingleton<ITemplateModule, Microservice.Modules.ReadModels.ReadModelsModule>();
        services.AddSingleton<ITemplateModule, Microservice.Modules.Tests.UnitTestModule>();
        services.AddSingleton<ITemplateModule, Microservice.Modules.Tests.IntegrationTestModule>();
        
        return services.BuildServiceProvider();
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
        
        // Generate microservice using Template Engine
        await GenerateMicroserviceAsync(config);
        
        Console.WriteLine($"📁 Files generated in: {Path.GetFullPath(options.OutputPath)}");
    }
    
    private TemplateConfiguration ApplyCustomizations(TemplateConfiguration config, GenerationOptions options)
    {
        // Update basic properties
        config.MicroserviceName = options.ServiceName;
        config.OutputPath = options.OutputPath;
        config.Namespace = $"{options.ServiceName}";
        
        // Set up project structure configuration
        config.ProjectStructure = new ProjectStructureConfiguration
        {
            SourceDirectory = "src",
            DomainProjectPath = "src/Domain",
            ApplicationProjectPath = "src/Application", 
            InfrastructureProjectPath = "src/Infrastructure",
            ApiProjectPath = "src/Api",
            TestsProjectPath = "tests"
        };
        
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
        Console.WriteLine($"🚀 Starting code generation with Template Engine...");
        
        // Create output directory
        Directory.CreateDirectory(config.OutputPath);
        
        // Create generation context
        var context = new GenerationContext(config);
        
        // Get Template Engine from DI
        var templateEngine = _serviceProvider.GetRequiredService<ITemplateEngine>();
        var logger = _serviceProvider.GetRequiredService<ILogger<GenerateService>>();
        
        try
        {
            // Generate using Template Engine
            await templateEngine.GenerateAsync(context, config);
            
            Console.WriteLine($"✅ Code generation completed successfully!");
            Console.WriteLine($"📊 Generated {context.GeneratedFiles.Count} files");
            
            // Show summary of generated files
            var filesByType = context.GeneratedFiles
                .GroupBy(f => Path.GetExtension(f.Path))
                .OrderBy(g => g.Key);
                
            foreach (var group in filesByType)
            {
                var extension = string.IsNullOrEmpty(group.Key) ? "no extension" : group.Key;
                Console.WriteLine($"   📄 {group.Count()} {extension} files");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during code generation");
            Console.WriteLine($"❌ Code generation failed: {ex.Message}");
            throw;
        }
    }
} 