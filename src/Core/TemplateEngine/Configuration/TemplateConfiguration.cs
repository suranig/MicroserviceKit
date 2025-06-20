namespace Microservice.Core.TemplateEngine.Configuration;

public class TemplateConfiguration
{
    public string MicroserviceName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string OutputPath { get; set; } = "./generated";
    
    public ArchitectureConfiguration? Architecture { get; set; }
    public FeaturesConfiguration? Features { get; set; }
    public DomainConfiguration? Domain { get; set; }
    public ProjectStructureConfiguration? ProjectStructure { get; set; }
    public DeploymentConfiguration? Deployment { get; set; }
    
    // Helper methods for simple access
    public string GetDatabaseProvider()
    {
        // Nowa struktura Database (enterprise)
        if (Features?.Database?.WriteModel?.Provider != null)
        {
            return Features.Database.WriteModel.Provider.ToLowerInvariant();
        }
        
        // Stara struktura Persistence (standard)
        if (Features?.Persistence?.Provider != null)
        {
            return Features.Persistence.Provider.ToLowerInvariant();
        }
        
        return "inmemory";
    }
    
    public string GetReadModelProvider()
    {
        // Nowa struktura Database (enterprise)
        if (Features?.Database?.ReadModel?.Provider != null)
        {
            var provider = Features.Database.ReadModel.Provider;
            return provider == "same" ? GetDatabaseProvider() : provider.ToLowerInvariant();
        }
        
        // Stara struktura Persistence (standard)
        if (Features?.Persistence?.ReadModel != null)
        {
            var readModel = Features.Persistence.ReadModel;
            return readModel == "same" ? GetDatabaseProvider() : readModel.ToLowerInvariant();
        }
        
        return GetDatabaseProvider();
    }
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
    public TestingConfiguration? Testing { get; set; }
    
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
    public List<EnumConfiguration>? Enums { get; set; }
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

public class EnumConfiguration
{
    public string Name { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new();
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
    public DockerConfiguration? Docker { get; set; }
    public KubernetesConfiguration? Kubernetes { get; set; }
}

public class DockerConfiguration
{
    public bool Enabled { get; set; } = false;
    public bool MultiStage { get; set; } = true;
    public bool HealthCheck { get; set; } = true;
}

public class KubernetesConfiguration
{
    public bool Enabled { get; set; } = false;
    public bool Deployment { get; set; } = true;
    public bool Service { get; set; } = true;
    public bool ConfigMap { get; set; } = true;
    public bool Secrets { get; set; } = true;
}

public class TestingConfiguration
{
    public string? Level { get; set; } = "unit"; // unit | integration | full | enterprise
    public string? Framework { get; set; } = "xunit"; // xunit | nunit | mstest
    public bool MockingEnabled { get; set; } = true;
    public bool TestContainersEnabled { get; set; } = false;
} 