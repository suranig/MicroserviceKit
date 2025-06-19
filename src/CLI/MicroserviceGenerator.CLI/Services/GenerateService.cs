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
        
        Console.WriteLine($"✅ Template loaded: {config.Name ?? config.MicroserviceName}");
        
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
        
        // Generate microservice using template engine
        var templateEngine = new MicroserviceGenerator();
        await templateEngine.GenerateAsync(config);
        
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
} 