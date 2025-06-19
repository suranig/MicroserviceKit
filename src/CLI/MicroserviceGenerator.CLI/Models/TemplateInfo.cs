using Microservice.Core.TemplateEngine.Configuration;

namespace MicroserviceGenerator.CLI.Models;

public class TemplateInfo
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public string EstimatedTime { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> WhenToUse { get; set; } = new();
    public List<string> Technologies { get; set; } = new();
    public int ProjectCount { get; set; }
    public TemplateConfiguration? Configuration { get; set; }
} 