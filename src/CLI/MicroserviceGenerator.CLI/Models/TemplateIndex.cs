namespace MicroserviceGenerator.CLI.Models;

public class TemplateIndex
{
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, TemplateCategory> Categories { get; set; } = new();
}

public class TemplateCategory
{
    public string Description { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<TemplateMetadata> Templates { get; set; } = new();
}

public class TemplateMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public string EstimatedTime { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> WhenToUse { get; set; } = new();
    public List<string> Technologies { get; set; } = new();
    public int ProjectCount { get; set; }
} 