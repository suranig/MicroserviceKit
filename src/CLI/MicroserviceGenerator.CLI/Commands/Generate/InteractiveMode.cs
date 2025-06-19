using MicroserviceGenerator.CLI.Models;
using MicroserviceGenerator.CLI.Services;

namespace MicroserviceGenerator.CLI.Commands.Generate;

public class InteractiveMode
{
    private readonly TemplateService _templateService;
    
    public InteractiveMode()
    {
        _templateService = new TemplateService();
    }
    
    public async Task<GenerationOptions> RunInteractiveAsync(GenerationOptions options)
    {
        Console.WriteLine($"🚀 Creating microservice: {options.ServiceName}");
        Console.WriteLine("🔧 Interactive Configuration Mode");
        Console.WriteLine();
        
        // Select template if not already specified
        if (string.IsNullOrEmpty(options.TemplateName))
        {
            options.TemplateName = await SelectTemplateAsync();
        }
        
        // Customize if requested
        if (await PromptYesNo("Customize template parameters?", defaultValue: false))
        {
            await CustomizeTemplateAsync(options);
        }
        
        // Confirm output path
        var outputPath = await PromptString("Output directory", options.OutputPath);
        if (!string.IsNullOrEmpty(outputPath))
        {
            options.OutputPath = outputPath;
        }
        
        Console.WriteLine();
        Console.WriteLine("📋 Configuration Summary:");
        Console.WriteLine($"   Service Name: {options.ServiceName}");
        Console.WriteLine($"   Template: {options.TemplateName}");
        Console.WriteLine($"   Output Path: {options.OutputPath}");
        
        if (options.CustomAggregates.Any())
            Console.WriteLine($"   Custom Aggregates: {string.Join(", ", options.CustomAggregates)}");
            
        if (options.ExternalServices.Any())
            Console.WriteLine($"   External Services: {string.Join(", ", options.ExternalServices)}");
            
        if (!string.IsNullOrEmpty(options.DatabaseProvider))
            Console.WriteLine($"   Database: {options.DatabaseProvider}");
            
        if (!string.IsNullOrEmpty(options.MessagingProvider))
            Console.WriteLine($"   Messaging: {options.MessagingProvider}");
        
        Console.WriteLine();
        
        if (!await PromptYesNo("Proceed with generation?", defaultValue: true))
        {
            Console.WriteLine("❌ Generation cancelled by user");
            Environment.Exit(0);
        }
        
        return options;
    }
    
    private async Task<string> SelectTemplateAsync()
    {
        var templates = await _templateService.LoadTemplatesAsync();
        
        if (!templates.Any())
        {
            Console.WriteLine("❌ No templates found!");
            Environment.Exit(1);
        }
        
        Console.WriteLine("📋 Available templates:");
        
        var categories = templates.GroupBy(t => t.Category).OrderBy(g => g.Key);
        var templateList = new List<TemplateInfo>();
        
        foreach (var categoryGroup in categories)
        {
            Console.WriteLine($"\n🏷️ {categoryGroup.Key.ToUpperInvariant()}:");
            
            foreach (var template in categoryGroup.OrderBy(t => t.Name))
            {
                templateList.Add(template);
                Console.WriteLine($"  {templateList.Count}. {template.Title} ({template.Complexity})");
                Console.WriteLine($"     {template.Description}");
            }
        }
        
        Console.WriteLine();
        
        while (true)
        {
            var choice = await PromptString($"Select template [1-{templateList.Count}]", "1");
            
            if (int.TryParse(choice, out int index) && index > 0 && index <= templateList.Count)
            {
                var selectedTemplate = templateList[index - 1];
                Console.WriteLine($"✅ Selected: {selectedTemplate.Title}");
                return selectedTemplate.Name;
            }
            
            Console.WriteLine($"❌ Invalid choice. Please enter a number between 1 and {templateList.Count}");
        }
    }
    
    private async Task CustomizeTemplateAsync(GenerationOptions options)
    {
        Console.WriteLine("\n🔧 Customization Options:");
        
        // Custom aggregates
        if (await PromptYesNo("Add custom aggregates?", defaultValue: false))
        {
            options.CustomAggregates = await PromptAggregatesAsync();
        }
        
        // External services
        if (await PromptYesNo("Configure external services?", defaultValue: false))
        {
            options.ExternalServices = await PromptExternalServicesAsync();
        }
        
        // Database provider
        var dbProvider = await PromptChoiceAsync("Database provider", 
            new[] { "postgresql", "sqlserver", "mysql", "sqlite", "inmemory" }, "postgresql");
        options.DatabaseProvider = dbProvider;
        
        // Messaging provider
        if (await PromptYesNo("Enable messaging?", defaultValue: true))
        {
            var msgProvider = await PromptChoiceAsync("Messaging provider",
                new[] { "rabbitmq", "servicebus", "inmemory" }, "rabbitmq");
            options.MessagingProvider = msgProvider;
        }
        
        // Authentication type
        var authType = await PromptChoiceAsync("Authentication type",
            new[] { "jwt", "oauth", "none" }, "jwt");
        options.AuthenticationType = authType;
        
        // API style
        var apiStyle = await PromptChoiceAsync("API style",
            new[] { "controllers", "minimal" }, "controllers");
        options.ApiStyle = apiStyle;
    }
    
    private async Task<List<string>> PromptAggregatesAsync()
    {
        var aggregates = new List<string>();
        
        Console.WriteLine("\n📦 Custom Aggregates (press Enter with empty name to finish):");
        
        while (true)
        {
            var name = await PromptString($"Aggregate #{aggregates.Count + 1} name", "");
            
            if (string.IsNullOrEmpty(name))
                break;
                
            if (aggregates.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"❌ Aggregate '{name}' already added");
                continue;
            }
            
            aggregates.Add(name);
            Console.WriteLine($"✅ Added aggregate: {name}");
        }
        
        return aggregates;
    }
    
    private async Task<List<string>> PromptExternalServicesAsync()
    {
        var services = new List<string>();
        
        Console.WriteLine("\n🌐 External Services (press Enter with empty name to finish):");
        
        while (true)
        {
            var name = await PromptString($"Service #{services.Count + 1} name", "");
            
            if (string.IsNullOrEmpty(name))
                break;
                
            if (services.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"❌ Service '{name}' already added");
                continue;
            }
            
            services.Add(name);
            Console.WriteLine($"✅ Added service: {name}");
        }
        
        return services;
    }
    
    private async Task<string> PromptChoiceAsync(string prompt, string[] choices, string defaultChoice)
    {
        Console.WriteLine($"\n{prompt}:");
        for (int i = 0; i < choices.Length; i++)
        {
            var marker = choices[i] == defaultChoice ? ">" : " ";
            Console.WriteLine($"  {marker} {i + 1}. {choices[i]}");
        }
        
        while (true)
        {
            var defaultIndex = Array.IndexOf(choices, defaultChoice) + 1;
            var choice = await PromptString($"Select [{defaultIndex}]", defaultIndex.ToString());
            
            if (int.TryParse(choice, out int index) && index > 0 && index <= choices.Length)
            {
                return choices[index - 1];
            }
            
            Console.WriteLine($"❌ Invalid choice. Please enter a number between 1 and {choices.Length}");
        }
    }
    
    private async Task<bool> PromptYesNo(string prompt, bool defaultValue = false)
    {
        var defaultText = defaultValue ? "Y/n" : "y/N";
        
        while (true)
        {
            var response = await PromptString($"{prompt} [{defaultText}]", "");
            
            if (string.IsNullOrEmpty(response))
                return defaultValue;
                
            var lower = response.ToLowerInvariant();
            if (lower == "y" || lower == "yes")
                return true;
            if (lower == "n" || lower == "no")
                return false;
                
            Console.WriteLine("❌ Please enter 'y' for yes or 'n' for no");
        }
    }
    
    private Task<string> PromptString(string prompt, string defaultValue)
    {
        var defaultText = !string.IsNullOrEmpty(defaultValue) ? $" [{defaultValue}]" : "";
        Console.Write($"{prompt}{defaultText}: ");
        
        var response = Console.ReadLine();
        
        if (string.IsNullOrEmpty(response))
            return Task.FromResult(defaultValue);
            
        return Task.FromResult(response.Trim());
    }
} 