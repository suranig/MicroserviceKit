namespace Microservice.Core.TemplateEngine.Configuration;

public class TemplateConfiguration
{
    public string MicroserviceName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string OutputPath { get; set; } = "./generated";
    
    public ArchitectureConfiguration? Architecture { get; set; }
    public FeaturesConfiguration? Features { get; set; }
    public DomainConfiguration? Domain { get; set; }
}

public class ArchitectureConfiguration
{
    public string? Level { get; set; } // minimal | standard | enterprise | auto
    public PatternsConfiguration? Patterns { get; set; }
}

public class PatternsConfiguration
{
    public string? DDD { get; set; } = "auto"; // auto | enabled | disabled
    public string? CQRS { get; set; } = "auto"; // auto | enabled | disabled
    public string? EventSourcing { get; set; } = "disabled";
}

public class FeaturesConfiguration
{
    public ApiConfiguration? Api { get; set; }
    public PersistenceConfiguration? Persistence { get; set; }
    public MessagingConfiguration? Messaging { get; set; }
    public ObservabilityConfiguration? Observability { get; set; }
    public DeploymentConfiguration? Deployment { get; set; }
    
    // Enterprise features
    public DatabaseConfiguration? Database { get; set; }
    public EnvironmentConfiguration? Environment { get; set; }
    public ExternalServicesConfiguration? ExternalServices { get; set; }
    public BackgroundJobsConfiguration? BackgroundJobs { get; set; }
}

public class DomainConfiguration
{
    public List<AggregateConfiguration>? Aggregates { get; set; }
    public List<ValueObjectConfiguration>? ValueObjects { get; set; }
}

public class AggregateConfiguration
{
    public string Name { get; set; } = string.Empty;
    public List<PropertyConfiguration> Properties { get; set; } = new();
    public List<string>? Operations { get; set; } // Zmienione z Methods na Operations
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

public class ApiConfiguration
{
    public string? Style { get; set; } = "auto"; // minimal | controllers | both | auto
    public string? Authentication { get; set; } = "none"; // none | jwt | oauth
    public string? Documentation { get; set; } = "auto"; // auto | swagger | none
}

public class PersistenceConfiguration
{
    public string? Provider { get; set; } = "inmemory"; // inmemory | sqlite | postgresql | sqlserver
    public string? Migrations { get; set; } = "auto"; // auto | enabled | disabled
    public string? ReadModel { get; set; } = "same"; // same | separate | redis
}

public class MessagingConfiguration
{
    public bool Enabled { get; set; } = false;
    public string? Provider { get; set; } = "inmemory"; // inmemory | rabbitmq | servicebus
    public List<string>? Patterns { get; set; } = new(); // outbox, saga, events
}

public class ObservabilityConfiguration
{
    public string? Logging { get; set; } = "auto"; // auto | serilog | none
    public string? Metrics { get; set; } = "auto"; // auto | prometheus | none
    public string? Tracing { get; set; } = "disabled"; // disabled | opentelemetry
}

public class DeploymentConfiguration
{
    public string? Docker { get; set; } = "auto"; // auto | enabled | disabled
    public string? Kubernetes { get; set; } = "disabled";
    public string? HealthChecks { get; set; } = "auto";
} 