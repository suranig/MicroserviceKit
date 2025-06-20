using System.Text.Json;
using MicroserviceGenerator.CLI.Models;
using Microservice.Core.TemplateEngine.Configuration;

namespace MicroserviceGenerator.CLI.Services;

public class TemplateService
{
    private readonly string _templatesPath;
    
    public TemplateService()
    {
        _templatesPath = FindTemplatesDirectory();
    }
    
    private static string FindTemplatesDirectory()
    {
        // Try multiple strategies to find the templates directory
        var strategies = new Func<string?>[]
        {
            // Strategy 1: Environment variable override
            () => Environment.GetEnvironmentVariable("MICROSERVICE_TEMPLATES_PATH"),
            
            // Strategy 2: Look for templates directory starting from assembly location
            () => FindTemplatesFromAssembly(),
            
            // Strategy 3: Look in current working directory
            () => {
                var currentDir = Directory.GetCurrentDirectory();
                var templatesPath = Path.Combine(currentDir, "templates");
                return Directory.Exists(templatesPath) ? templatesPath : null;
            },
            
            // Strategy 4: Look in parent directories (up to 6 levels)
            () => FindTemplatesInParentDirectories(),
            
            // Strategy 5: Default fallback path
            () => Path.Combine(AppContext.BaseDirectory, "templates")
        };
        
        foreach (var strategy in strategies)
        {
            var path = strategy();
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                return Path.GetFullPath(path);
            }
        }
        
        // If nothing found, return a default path (will be created if needed)
        return Path.Combine(AppContext.BaseDirectory, "templates");
    }
    
    private static string? FindTemplatesFromAssembly()
    {
        try
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            
            if (string.IsNullOrEmpty(assemblyDirectory))
                return null;
            
            // Look for templates directory by walking up the directory tree
            var currentDir = new DirectoryInfo(assemblyDirectory);
            while (currentDir != null && currentDir.Parent != null)
            {
                var templatesPath = Path.Combine(currentDir.FullName, "templates");
                if (Directory.Exists(templatesPath))
                {
                    return templatesPath;
                }
                currentDir = currentDir.Parent;
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }
    
    private static string? FindTemplatesInParentDirectories()
    {
        try
        {
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var maxLevels = 6;
            var level = 0;
            
            while (currentDir != null && level < maxLevels)
            {
                var templatesPath = Path.Combine(currentDir.FullName, "templates");
                if (Directory.Exists(templatesPath))
                {
                    return templatesPath;
                }
                currentDir = currentDir.Parent;
                level++;
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }
    
    public async Task<List<TemplateInfo>> LoadTemplatesAsync()
    {
        var indexPath = Path.Combine(_templatesPath, "index.json");
        
        if (!File.Exists(indexPath))
        {
            // If index doesn't exist, scan for templates manually
            return await ScanTemplatesAsync();
        }
        
        try
        {
            var json = await File.ReadAllTextAsync(indexPath);
            
            var index = JsonSerializer.Deserialize<TemplateIndex>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (index == null)
            {
                throw new InvalidOperationException("Failed to deserialize template index");
            }
            
            var templates = new List<TemplateInfo>();
            foreach (var category in index.Categories)
            {
                foreach (var template in category.Value.Templates)
                {
                    var templatePath = Path.Combine(_templatesPath, category.Value.Path, template.Name);
                    
                    templates.Add(new TemplateInfo
                    {
                        Name = template.Name,
                        Title = template.Title,
                        Description = template.Description,
                        Category = category.Key,
                        Path = templatePath,
                        Complexity = template.Complexity,
                        EstimatedTime = template.EstimatedTime,
                        Features = template.Features,
                        Tags = template.Tags,
                        WhenToUse = template.WhenToUse,
                        Technologies = template.Technologies,
                        ProjectCount = template.ProjectCount?.ToString() ?? "1"
                    });
                }
            }
            
            return templates;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load template index: {ex.Message}", ex);
        }
    }
    
    public async Task<TemplateInfo?> LoadTemplateAsync(string templateName)
    {
        var templates = await LoadTemplatesAsync();
        return templates.FirstOrDefault(t => 
            t.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase) ||
            t.Title.Equals(templateName, StringComparison.OrdinalIgnoreCase));
    }
    
    public async Task<TemplateConfiguration?> LoadTemplateConfigurationAsync(string templateName)
    {
        var templateInfo = await LoadTemplateAsync(templateName);
        if (templateInfo == null)
        {
            // Try to find template by direct path
            var templatePath = await FindTemplatePathAsync(templateName);
            if (templatePath == null)
                return null;
                
            return await LoadConfigurationFromPathAsync(templatePath);
        }
        
        return await LoadConfigurationFromPathAsync(templateInfo.Path);
    }
    
    private Task<string?> FindTemplatePathAsync(string templateName)
    {
        // Try different possible paths
        var possiblePaths = new[]
        {
            Path.Combine(_templatesPath, $"{templateName}.json"),
            Path.Combine(_templatesPath, "service-types", $"{templateName}.json"),
            Path.Combine(_templatesPath, "architecture-levels", $"{templateName}.json"),
            Path.Combine(_templatesPath, "examples", $"{templateName}.json"),
            templateName // Direct path
        };
        
        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return Task.FromResult<string?>(path);
            }
        }
        
        return Task.FromResult<string?>(null);
    }
    
    private async Task<TemplateConfiguration?> LoadConfigurationFromPathAsync(string templatePath)
    {
        if (!File.Exists(templatePath))
            return null;
            
        try
        {
            var json = await File.ReadAllTextAsync(templatePath);
            return JsonSerializer.Deserialize<TemplateConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load template configuration from {templatePath}: {ex.Message}", ex);
        }
    }
    
    private async Task<List<TemplateInfo>> ScanTemplatesAsync()
    {
        var templates = new List<TemplateInfo>();
        
        if (!Directory.Exists(_templatesPath))
        {
            return templates;
        }
        
        // Scan for JSON files in templates directory
        var jsonFiles = Directory.GetFiles(_templatesPath, "*.json", SearchOption.AllDirectories);
        
        foreach (var file in jsonFiles)
        {
            if (Path.GetFileName(file) == "index.json")
                continue;
                
            try
            {
                var config = await LoadConfigurationFromPathAsync(file);
                if (config != null)
                {
                    var relativePath = Path.GetRelativePath(_templatesPath, file);
                    var category = Path.GetDirectoryName(relativePath) ?? "unknown";
                    
                    templates.Add(new TemplateInfo
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Title = config.MicroserviceName ?? "Unknown",
                        Description = "No description available",
                        Category = category,
                        Path = file,
                        Complexity = DetermineComplexity(config),
                        EstimatedTime = "Unknown",
                        Features = ExtractFeatures(config),
                        Tags = new List<string>(),
                        WhenToUse = new List<string>(),
                        Technologies = ExtractTechnologies(config),
                        ProjectCount = "1"
                    });
                }
            }
            catch
            {
                // Skip invalid template files
                continue;
            }
        }
        
        return templates;
    }
    
    private static string DetermineComplexity(TemplateConfiguration config)
    {
        var complexity = 0;
        
        if (config.Domain?.Aggregates?.Any() == true) complexity++;
        if (config.Features?.Messaging?.Enabled == true) complexity++;
        if (config.Features?.ExternalServices?.Enabled == true) complexity++;
        if (config.Architecture?.Patterns?.EventSourcing == "enabled") complexity++;
        if (config.Features?.Testing?.Level == "enterprise") complexity++;
        
        return complexity switch
        {
            0 or 1 => "minimal",
            2 or 3 => "standard",
            _ => "enterprise"
        };
    }
    
    private static List<string> ExtractFeatures(TemplateConfiguration config)
    {
        var features = new List<string>();
        
        if (config.Architecture?.Patterns?.DDD == "enabled") features.Add("ddd");
        if (config.Architecture?.Patterns?.CQRS == "enabled") features.Add("cqrs");
        if (config.Architecture?.Patterns?.EventSourcing == "enabled") features.Add("event-sourcing");
        if (config.Features?.Messaging?.Enabled == true) features.Add("messaging");
        if (config.Features?.ExternalServices?.Enabled == true) features.Add("external-services");
        if (config.Features?.Database?.Cache?.Enabled == true) features.Add("caching");
        
        return features;
    }
    
    private static List<string> ExtractTechnologies(TemplateConfiguration config)
    {
        var technologies = new List<string>();
        
        if (config.Features?.Database?.WriteModel?.Provider != null)
        {
            technologies.Add(config.Features.Database.WriteModel.Provider);
        }
        
        if (config.Features?.Database?.ReadModel?.Provider != null)
        {
            technologies.Add(config.Features.Database.ReadModel.Provider);
        }
        
        if (config.Features?.Messaging?.Provider != null)
        {
            technologies.Add(config.Features.Messaging.Provider);
        }
        
        if (config.Features?.Database?.Cache?.Provider != null)
        {
            technologies.Add(config.Features.Database.Cache.Provider);
        }
        
        return technologies;
    }
} 