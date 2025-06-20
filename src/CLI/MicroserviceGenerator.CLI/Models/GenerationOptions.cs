namespace MicroserviceGenerator.CLI.Models;

public class GenerationOptions
{
    public string ServiceName { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public bool Interactive { get; set; }
    public bool Customize { get; set; }
    
    // Customization options
    public List<string> CustomAggregates { get; set; } = new();
    public List<string> ExternalServices { get; set; } = new();
    public string? DatabaseProvider { get; set; }
    public string? MessagingProvider { get; set; }
    public string? AuthenticationType { get; set; }
    public string? ApiStyle { get; set; }
} 