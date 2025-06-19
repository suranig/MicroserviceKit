using MicroserviceGenerator.CLI.Models;
using Microservice.Core.TemplateEngine.Configuration;

namespace MicroserviceGenerator.CLI.Services;

public class ValidationService
{
    public ValidationResult ValidateTemplate(TemplateConfiguration config)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Validate basic properties
        if (string.IsNullOrEmpty(config.MicroserviceName))
            errors.Add("MicroserviceName is required");
            
        if (string.IsNullOrEmpty(config.Namespace))
            warnings.Add("Namespace not specified, will use default naming convention");
            
        // Validate microservice name format
        if (!string.IsNullOrEmpty(config.MicroserviceName))
        {
            if (!IsValidServiceName(config.MicroserviceName))
                errors.Add("MicroserviceName must be a valid C# identifier (letters, digits, underscore, no spaces)");
        }
        
        // Validate domain configuration
        if (config.Domain != null)
        {
            ValidateDomainConfiguration(config.Domain, errors, warnings);
        }
        else
        {
            warnings.Add("No domain configuration specified, will generate minimal domain structure");
        }
        
        // Validate features configuration
        if (config.Features != null)
        {
            ValidateFeaturesConfiguration(config.Features, errors, warnings);
        }
        
        // Validate architecture configuration
        if (config.Architecture != null)
        {
            ValidateArchitectureConfiguration(config.Architecture, errors, warnings);
        }
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            Warnings = warnings
        };
    }
    
    public ValidationResult ValidateGenerationOptions(GenerationOptions options)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Validate service name
        if (string.IsNullOrEmpty(options.ServiceName))
            errors.Add("Service name is required");
        else if (!IsValidServiceName(options.ServiceName))
            errors.Add("Service name must be a valid C# identifier (letters, digits, underscore, no spaces)");
            
        // Validate template name
        if (string.IsNullOrEmpty(options.TemplateName))
            errors.Add("Template name is required");
            
        // Validate output path
        if (string.IsNullOrEmpty(options.OutputPath))
            errors.Add("Output path is required");
        else
        {
            try
            {
                Path.GetFullPath(options.OutputPath);
            }
            catch
            {
                errors.Add("Output path is not valid");
            }
        }
        
        // Validate custom aggregates
        if (options.CustomAggregates.Any())
        {
            foreach (var aggregate in options.CustomAggregates)
            {
                if (string.IsNullOrEmpty(aggregate))
                    errors.Add("Aggregate name cannot be empty");
                else if (!IsValidClassName(aggregate))
                    errors.Add($"Aggregate name '{aggregate}' must be a valid C# class name");
            }
        }
        
        // Validate external services
        if (options.ExternalServices.Any())
        {
            foreach (var service in options.ExternalServices)
            {
                if (string.IsNullOrEmpty(service))
                    errors.Add("External service name cannot be empty");
                else if (!IsValidClassName(service))
                    errors.Add($"External service name '{service}' must be a valid C# class name");
            }
        }
        
        // Validate database provider
        if (!string.IsNullOrEmpty(options.DatabaseProvider))
        {
            var validProviders = new[] { "postgresql", "sqlserver", "mysql", "sqlite", "inmemory" };
            if (!validProviders.Contains(options.DatabaseProvider.ToLowerInvariant()))
                errors.Add($"Database provider '{options.DatabaseProvider}' is not supported. Valid options: {string.Join(", ", validProviders)}");
        }
        
        // Validate messaging provider
        if (!string.IsNullOrEmpty(options.MessagingProvider))
        {
            var validProviders = new[] { "rabbitmq", "servicebus", "inmemory" };
            if (!validProviders.Contains(options.MessagingProvider.ToLowerInvariant()))
                errors.Add($"Messaging provider '{options.MessagingProvider}' is not supported. Valid options: {string.Join(", ", validProviders)}");
        }
        
        // Validate authentication type
        if (!string.IsNullOrEmpty(options.AuthenticationType))
        {
            var validTypes = new[] { "jwt", "oauth", "none" };
            if (!validTypes.Contains(options.AuthenticationType.ToLowerInvariant()))
                errors.Add($"Authentication type '{options.AuthenticationType}' is not supported. Valid options: {string.Join(", ", validTypes)}");
        }
        
        // Validate API style
        if (!string.IsNullOrEmpty(options.ApiStyle))
        {
            var validStyles = new[] { "controllers", "minimal" };
            if (!validStyles.Contains(options.ApiStyle.ToLowerInvariant()))
                errors.Add($"API style '{options.ApiStyle}' is not supported. Valid options: {string.Join(", ", validStyles)}");
        }
        
        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            Warnings = warnings
        };
    }
    
    private static void ValidateDomainConfiguration(DomainConfiguration domain, List<string> errors, List<string> warnings)
    {
        if (domain.Aggregates == null || !domain.Aggregates.Any())
        {
            warnings.Add("No aggregates defined, will generate default aggregate");
            return;
        }
        
        foreach (var aggregate in domain.Aggregates)
        {
            if (string.IsNullOrEmpty(aggregate.Name))
                errors.Add("Aggregate name is required");
            else if (!IsValidClassName(aggregate.Name))
                errors.Add($"Aggregate name '{aggregate.Name}' must be a valid C# class name");
                
            if (aggregate.Properties != null)
            {
                foreach (var property in aggregate.Properties)
                {
                    if (string.IsNullOrEmpty(property.Name))
                        errors.Add($"Property name is required in aggregate '{aggregate.Name}'");
                    else if (!IsValidPropertyName(property.Name))
                        errors.Add($"Property name '{property.Name}' in aggregate '{aggregate.Name}' must be a valid C# property name");
                        
                    if (string.IsNullOrEmpty(property.Type))
                        errors.Add($"Property type is required for '{property.Name}' in aggregate '{aggregate.Name}'");
                }
            }
        }
    }
    
    private static void ValidateFeaturesConfiguration(FeaturesConfiguration features, List<string> errors, List<string> warnings)
    {
        // Validate database configuration
        if (features.Database != null)
        {
            if (features.Database.WriteModel?.Provider != null)
            {
                var validProviders = new[] { "postgresql", "sqlserver", "mysql", "sqlite", "inmemory" };
                if (!validProviders.Contains(features.Database.WriteModel.Provider.ToLowerInvariant()))
                    errors.Add($"Write model database provider '{features.Database.WriteModel.Provider}' is not supported");
            }
            
            if (features.Database.ReadModel?.Provider != null)
            {
                var validProviders = new[] { "mongodb", "postgresql", "sqlserver", "mysql", "sqlite", "inmemory" };
                if (!validProviders.Contains(features.Database.ReadModel.Provider.ToLowerInvariant()))
                    errors.Add($"Read model database provider '{features.Database.ReadModel.Provider}' is not supported");
            }
        }
        
        // Validate messaging configuration
        if (features.Messaging?.Enabled == true && string.IsNullOrEmpty(features.Messaging.Provider))
        {
            warnings.Add("Messaging is enabled but no provider specified, will use default (RabbitMQ)");
        }
        
        // Validate external services configuration
        if (features.ExternalServices?.Enabled == true)
        {
            if (features.ExternalServices.Services == null || !features.ExternalServices.Services.Any())
            {
                warnings.Add("External services are enabled but no services defined");
            }
            else
            {
                foreach (var service in features.ExternalServices.Services)
                {
                    if (string.IsNullOrEmpty(service.Name))
                        errors.Add("External service name is required");
                    else if (!IsValidClassName(service.Name))
                        errors.Add($"External service name '{service.Name}' must be a valid C# class name");
                }
            }
        }
    }
    
    private static void ValidateArchitectureConfiguration(ArchitectureConfiguration architecture, List<string> errors, List<string> warnings)
    {
        if (architecture.Patterns != null)
        {
            // Validate pattern combinations
            if (architecture.Patterns.EventSourcing == "enabled" && architecture.Patterns.CQRS != "enabled")
            {
                warnings.Add("Event Sourcing is typically used with CQRS pattern");
            }
        }
    }
    
    private static bool IsValidServiceName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
            
        // Service name should be a valid C# identifier
        return IsValidClassName(name);
    }
    
    private static bool IsValidClassName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
            
        // Must start with letter or underscore
        if (!char.IsLetter(name[0]) && name[0] != '_')
            return false;
            
        // Rest must be letters, digits, or underscores
        return name.Skip(1).All(c => char.IsLetterOrDigit(c) || c == '_');
    }
    
    private static bool IsValidPropertyName(string name)
    {
        // Property names follow same rules as class names
        return IsValidClassName(name);
    }
} 