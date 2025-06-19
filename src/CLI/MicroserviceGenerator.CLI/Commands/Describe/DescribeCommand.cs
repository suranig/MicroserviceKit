using System.CommandLine;
using MicroserviceGenerator.CLI.Services;

namespace MicroserviceGenerator.CLI.Commands.Describe;

public static class DescribeCommand
{
    public static Command Create()
    {
        var command = new Command("describe", "Show detailed information about a template");
        
        var templateArgument = new Argument<string>("template", "Template name or path");
        var formatOption = new Option<string>("--format", () => "table", "Output format (table, json, markdown)");
        
        command.AddArgument(templateArgument);
        command.AddOption(formatOption);
        
        command.SetHandler(async (template, format) =>
        {
            await ExecuteDescribeAsync(template, format);
        }, templateArgument, formatOption);
        
        return command;
    }
    
    private static async Task ExecuteDescribeAsync(string template, string format)
    {
        try
        {
            var templateService = new TemplateService();
            var templateInfo = await templateService.LoadTemplateAsync(template);
            
            if (templateInfo == null)
            {
                Console.WriteLine($"❌ Template not found: {template}");
                Console.WriteLine("\nUse 'microkit list templates' to see available templates.");
                Environment.Exit(1);
                return;
            }
            
            switch (format.ToLowerInvariant())
            {
                case "json":
                    await ShowJsonDescriptionAsync(templateInfo);
                    break;
                case "markdown":
                    await ShowMarkdownDescriptionAsync(templateInfo);
                    break;
                default:
                    await ShowTableDescriptionAsync(templateInfo);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error describing template: {ex.Message}");
            Environment.Exit(1);
        }
    }
    
    private static Task ShowTableDescriptionAsync(Models.TemplateInfo templateInfo)
    {
        Console.WriteLine($"📋 Template: {templateInfo.Title}");
        Console.WriteLine($"   Name: {templateInfo.Name}");
        Console.WriteLine($"   Category: {templateInfo.Category}");
        Console.WriteLine($"   Complexity: {templateInfo.Complexity}");
        Console.WriteLine($"   Estimated Time: {templateInfo.EstimatedTime}");
        Console.WriteLine();
        
        Console.WriteLine($"📝 Description:");
        Console.WriteLine($"   {templateInfo.Description}");
        Console.WriteLine();
        
        if (templateInfo.WhenToUse.Any())
        {
            Console.WriteLine($"🎯 When to use:");
            foreach (var use in templateInfo.WhenToUse)
            {
                Console.WriteLine($"   • {use}");
            }
            Console.WriteLine();
        }
        
        if (templateInfo.Features.Any())
        {
            Console.WriteLine($"✨ Features:");
            foreach (var feature in templateInfo.Features)
            {
                Console.WriteLine($"   • {feature}");
            }
            Console.WriteLine();
        }
        
        if (templateInfo.Technologies.Any())
        {
            Console.WriteLine($"🛠️ Technologies:");
            foreach (var tech in templateInfo.Technologies)
            {
                Console.WriteLine($"   • {tech}");
            }
            Console.WriteLine();
        }
        
        if (templateInfo.Tags.Any())
        {
            Console.WriteLine($"🏷️ Tags: {string.Join(", ", templateInfo.Tags)}");
            Console.WriteLine();
        }
        
        Console.WriteLine($"📊 Project Count: {templateInfo.ProjectCount}");
        
        return Task.CompletedTask;
    }
    
    private static Task ShowJsonDescriptionAsync(Models.TemplateInfo templateInfo)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(templateInfo, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        
        Console.WriteLine(json);
        return Task.CompletedTask;
    }
    
    private static Task ShowMarkdownDescriptionAsync(Models.TemplateInfo templateInfo)
    {
        Console.WriteLine($"# {templateInfo.Title}");
        Console.WriteLine();
        Console.WriteLine($"**Name:** {templateInfo.Name}");
        Console.WriteLine($"**Category:** {templateInfo.Category}");
        Console.WriteLine($"**Complexity:** {templateInfo.Complexity}");
        Console.WriteLine($"**Estimated Time:** {templateInfo.EstimatedTime}");
        Console.WriteLine();
        
        Console.WriteLine("## Description");
        Console.WriteLine(templateInfo.Description);
        Console.WriteLine();
        
        if (templateInfo.WhenToUse.Any())
        {
            Console.WriteLine("## When to Use");
            foreach (var use in templateInfo.WhenToUse)
            {
                Console.WriteLine($"- {use}");
            }
            Console.WriteLine();
        }
        
        if (templateInfo.Features.Any())
        {
            Console.WriteLine("## Features");
            foreach (var feature in templateInfo.Features)
            {
                Console.WriteLine($"- {feature}");
            }
            Console.WriteLine();
        }
        
        if (templateInfo.Technologies.Any())
        {
            Console.WriteLine("## Technologies");
            foreach (var tech in templateInfo.Technologies)
            {
                Console.WriteLine($"- {tech}");
            }
            Console.WriteLine();
        }
        
        if (templateInfo.Tags.Any())
        {
            Console.WriteLine($"**Tags:** {string.Join(", ", templateInfo.Tags)}");
            Console.WriteLine();
        }
        
        Console.WriteLine($"**Project Count:** {templateInfo.ProjectCount}");
        
        return Task.CompletedTask;
    }
} 