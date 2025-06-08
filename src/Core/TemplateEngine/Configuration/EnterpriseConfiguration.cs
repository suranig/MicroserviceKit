namespace Microservice.Core.TemplateEngine.Configuration;

// Rozszerzenie istniejącej konfiguracji o enterprise features
public class DatabaseConfiguration
{
    public WriteModelConfiguration? WriteModel { get; set; }
    public ReadModelConfiguration? ReadModel { get; set; }
    public CacheConfiguration? Cache { get; set; }
    public EventStoreConfiguration? EventStore { get; set; }
}

public class WriteModelConfiguration
{
    public string Provider { get; set; } = "postgresql"; // postgresql | mysql | sqlserver | sqlite
    public string? ConnectionString { get; set; }
    public bool EnableMigrations { get; set; } = true;
    public bool EnableAuditing { get; set; } = false;
    public bool EnableSoftDelete { get; set; } = false;
}

public class ReadModelConfiguration
{
    public string Provider { get; set; } = "same"; // same | mongodb | elasticsearch | cosmosdb
    public string? ConnectionString { get; set; }
    public bool EnableProjections { get; set; } = true;
    public string SyncStrategy { get; set; } = "eventual"; // eventual | immediate | batch
}

public class CacheConfiguration
{
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "redis"; // redis | inmemory | distributed
    public string? ConnectionString { get; set; }
    public int DefaultTtlMinutes { get; set; } = 60;
    public List<string> CachedQueries { get; set; } = new();
}

public class EventStoreConfiguration
{
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "postgresql"; // postgresql | eventstore | cosmosdb
    public string? ConnectionString { get; set; }
    public bool EnableSnapshots { get; set; } = true;
    public int SnapshotFrequency { get; set; } = 10;
}

public class EnvironmentConfiguration
{
    public bool UseDotEnv { get; set; } = true;
    public bool UseKeyVault { get; set; } = false;
    public string? KeyVaultUrl { get; set; }
    public bool UseConsul { get; set; } = false;
    public string? ConsulUrl { get; set; }
    public List<EnvironmentVariable> Variables { get; set; } = new();
}

public class EnvironmentVariable
{
    public string Name { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public bool IsSecret { get; set; } = false;
    public string? Description { get; set; }
}

public class ExternalServicesConfiguration
{
    public bool Enabled { get; set; } = false;
    public List<ExternalServiceConfiguration> Services { get; set; } = new();
    public ResilienceConfiguration Resilience { get; set; } = new();
}

public class ExternalServiceConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Type { get; set; } = "http"; // http | grpc | graphql
    public AuthenticationConfiguration? Authentication { get; set; }
    public List<string> Operations { get; set; } = new();
}

public class AuthenticationConfiguration
{
    public string Type { get; set; } = "none"; // none | apikey | bearer | oauth2 | certificate
    public Dictionary<string, string> Settings { get; set; } = new();
}

public class ResilienceConfiguration
{
    public RetryConfiguration Retry { get; set; } = new();
    public CircuitBreakerConfiguration CircuitBreaker { get; set; } = new();
    public TimeoutConfiguration Timeout { get; set; } = new();
    public BulkheadConfiguration Bulkhead { get; set; } = new();
}

public class RetryConfiguration
{
    public bool Enabled { get; set; } = true;
    public int MaxAttempts { get; set; } = 3;
    public string Strategy { get; set; } = "exponential"; // exponential | linear | immediate
    public int BaseDelayMs { get; set; } = 1000;
}

public class CircuitBreakerConfiguration
{
    public bool Enabled { get; set; } = true;
    public int FailureThreshold { get; set; } = 5;
    public int TimeoutMs { get; set; } = 30000;
    public int RecoveryTimeMs { get; set; } = 60000;
}

public class TimeoutConfiguration
{
    public bool Enabled { get; set; } = true;
    public int DefaultTimeoutMs { get; set; } = 30000;
}

public class BulkheadConfiguration
{
    public bool Enabled { get; set; } = false;
    public int MaxConcurrency { get; set; } = 10;
    public int QueueCapacity { get; set; } = 100;
}

public class BackgroundJobsConfiguration
{
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "hangfire"; // hangfire | quartz | hosted-service
    public string? ConnectionString { get; set; }
    public List<JobConfiguration> Jobs { get; set; } = new();
}

public class JobConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty; // cron expression
    public string Type { get; set; } = "recurring"; // recurring | delayed | fire-and-forget
    public Dictionary<string, object> Parameters { get; set; } = new();
} 