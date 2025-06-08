namespace Microservice.Core.TemplateEngine.Configuration;

public class TemplateConfiguration
{
    public string MicroserviceName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string OutputPath { get; set; } = "./";
    
    public DDDConfiguration DDD { get; set; } = new();
    public CQRSConfiguration CQRS { get; set; } = new();
    public APIConfiguration API { get; set; } = new();
    public PersistenceConfiguration Persistence { get; set; } = new();
    public MessagingConfiguration Messaging { get; set; } = new();
    public ContainerizationConfiguration Containerization { get; set; } = new();
}

public class DDDConfiguration
{
    public bool Enabled { get; set; } = true;
    public List<AggregateConfiguration> Aggregates { get; set; } = new();
    public List<ValueObjectConfiguration> ValueObjects { get; set; } = new();
}

public class AggregateConfiguration
{
    public string Name { get; set; } = string.Empty;
    public List<PropertyConfiguration> Properties { get; set; } = new();
    public List<string> Methods { get; set; } = new();
}

public class PropertyConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
}

public class ValueObjectConfiguration
{
    public string Name { get; set; } = string.Empty;
    public List<PropertyConfiguration> Properties { get; set; } = new();
}

public class CQRSConfiguration
{
    public bool Enabled { get; set; } = true;
    public string Mediator { get; set; } = "wolverine";
}

public class APIConfiguration
{
    public List<string> Types { get; set; } = new() { "rest" };
    public string Authentication { get; set; } = "none";
}

public class PersistenceConfiguration
{
    public string WriteModel { get; set; } = "inmemory";
    public string ReadModel { get; set; } = "inmemory";
}

public class MessagingConfiguration
{
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "inmemory";
}

public class ContainerizationConfiguration
{
    public bool Docker { get; set; } = false;
    public bool Kubernetes { get; set; } = false;
} 