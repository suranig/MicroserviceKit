using System.CommandLine;
using MicroserviceGenerator.CLI.Services;

namespace MicroserviceGenerator.CLI.Commands.List;

public static class ListTemplatesCommand
{
    public static Command Create()
    {
        var command = new Command("list", "List available microservice templates");
        
        var categoryOption = new Option<string?>("--category", "Filter by category");
        var tagOption = new Option<string?>("--tag", "Filter by tag");
        var complexityOption = new Option<string?>("--complexity", "Filter by complexity");
        var formatOption = new Option<string>("--format", () => "table", "Output format (table, json, markdown)");
        var detailedOption = new Option<bool>("--detailed", () => false, "Show detailed information");
        
        command.AddOption(categoryOption);
        command.AddOption(tagOption);
        command.AddOption(complexityOption);
        command.AddOption(formatOption);
        command.AddOption(detailedOption);
        
        command.SetHandler(async (category, tag, complexity, format, detailed) =>
        {
            await ExecuteListAsync(category, tag, complexity, format, detailed);
        }, categoryOption, tagOption, complexityOption, formatOption, detailedOption);
        
        return command;
    }
    
    private static async Task ExecuteListAsync(string? category, string? tag, string? complexity, string format, bool detailed)
    {
        try
        {
            var templateService = new TemplateService();
            var templates = await templateService.LoadTemplatesAsync();
            
            // Apply filters
            if (!string.IsNullOrEmpty(category))
            {
                templates = templates.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            if (!string.IsNullOrEmpty(tag))
            {
                templates = templates.Where(t => t.Tags.Any(tg => tg.Equals(tag, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            
            if (!string.IsNullOrEmpty(complexity))
            {
                templates = templates.Where(t => t.Complexity.Equals(complexity, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            if (!templates.Any())
            {
                Console.WriteLine("❌ No templates found matching the specified criteria.");
                return;
            }
            
            switch (format.ToLowerInvariant())
            {
                case "json":
                    await ShowJsonListAsync(templates, detailed);
                    break;
                case "markdown":
                    await ShowMarkdownListAsync(templates, detailed);
                    break;
                default:
                    await ShowTableListAsync(templates, detailed);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error listing templates: {ex.Message}");
            Environment.Exit(1);
        }
    }
    
    private static Task ShowTableListAsync(List<Models.TemplateInfo> templates, bool detailed)
    {
        Console.WriteLine("📋 Available Templates:");
        Console.WriteLine();
        
        var categories = templates.GroupBy(t => t.Category).OrderBy(g => g.Key);
        
        foreach (var categoryGroup in categories)
        {
            Console.WriteLine($"🏷️ {categoryGroup.Key.ToUpperInvariant()}");
            Console.WriteLine();
            
            foreach (var template in categoryGroup.OrderBy(t => t.Name))
            {
                Console.WriteLine($"  📦 {template.Name}");
                Console.WriteLine($"     {template.Title} ({template.Complexity})");
                Console.WriteLine($"     {template.Description}");
                
                if (detailed)
                {
                    Console.WriteLine($"     ⏱️ Time: {template.EstimatedTime}");
                    
                    if (template.Features.Any())
                    {
                        Console.WriteLine($"     ✨ Features: {string.Join(", ", template.Features)}");
                    }
                    
                    if (template.Technologies.Any())
                    {
                        Console.WriteLine($"     🛠️ Tech: {string.Join(", ", template.Technologies)}");
                    }
                    
                    if (template.Tags.Any())
                    {
                        Console.WriteLine($"     🏷️ Tags: {string.Join(", ", template.Tags)}");
                    }
                }
                
                Console.WriteLine();
            }
        }
        
        Console.WriteLine($"📊 Total: {templates.Count} templates");
        Console.WriteLine();
        Console.WriteLine("💡 Use 'microkit describe <template-name>' for detailed information");
        Console.WriteLine("💡 Use 'microkit generate <service-name> --template <template-name>' to generate");
        
        return Task.CompletedTask;
    }
    
    private static Task ShowJsonListAsync(List<Models.TemplateInfo> templates, bool detailed)
    {
        var output = detailed ? templates : templates.Select(t => new
        {
            t.Name,
            t.Title,
            t.Description,
            t.Category,
            t.Complexity,
            t.EstimatedTime
        });
        
        var json = System.Text.Json.JsonSerializer.Serialize(output, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        
        Console.WriteLine(json);
        return Task.CompletedTask;
    }
    
    private static Task ShowMarkdownListAsync(List<Models.TemplateInfo> templates, bool detailed)
    {
        Console.WriteLine("# Available Templates");
        Console.WriteLine();
        
        var categories = templates.GroupBy(t => t.Category).OrderBy(g => g.Key);
        
        foreach (var categoryGroup in categories)
        {
            Console.WriteLine($"## {categoryGroup.Key}");
            Console.WriteLine();
            
            foreach (var template in categoryGroup.OrderBy(t => t.Name))
            {
                Console.WriteLine($"### {template.Name}");
                Console.WriteLine();
                Console.WriteLine($"**{template.Title}** ({template.Complexity})");
                Console.WriteLine();
                Console.WriteLine(template.Description);
                Console.WriteLine();
                
                if (detailed)
                {
                    Console.WriteLine($"- **Estimated Time:** {template.EstimatedTime}");
                    
                    if (template.Features.Any())
                    {
                        Console.WriteLine($"- **Features:** {string.Join(", ", template.Features)}");
                    }
                    
                    if (template.Technologies.Any())
                    {
                        Console.WriteLine($"- **Technologies:** {string.Join(", ", template.Technologies)}");
                    }
                    
                    if (template.Tags.Any())
                    {
                        Console.WriteLine($"- **Tags:** {string.Join(", ", template.Tags)}");
                    }
                    
                    Console.WriteLine();
                }
            }
        }
        
        Console.WriteLine($"**Total:** {templates.Count} templates");
        
        return Task.CompletedTask;
    }
} 