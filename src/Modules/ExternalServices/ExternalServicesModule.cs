using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.ExternalServices;

public class ExternalServicesModule : ITemplateModule
{
    public string Name => "ExternalServices";
    public string Description => "Generates HTTP clients with Polly resilience patterns and service registry";

    public bool IsEnabled(TemplateConfiguration config)
    {
        return config.Features?.ExternalServices?.Enabled == true;
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var externalServicesConfig = config.Features?.ExternalServices;

        if (externalServicesConfig?.Services != null)
        {
            foreach (var service in externalServicesConfig.Services)
            {
                await GenerateServiceClientAsync(context, service);
                await GenerateServiceInterfaceAsync(context, service);
            }
        }

        // Generate resilience infrastructure
        await GenerateResilienceInfrastructureAsync(context);
        await GenerateServiceRegistryAsync(context);
        await GenerateExternalServicesExtensionsAsync(context);
    }

    private async Task GenerateServiceClientAsync(GenerationContext context, ExternalServiceConfiguration service)
    {
        var config = context.Configuration;
        var infrastructurePath = context.GetInfrastructureProjectPath();

        // Create external services directory
        var externalServicesPath = Path.Combine(infrastructurePath, "ExternalServices", service.Name);
        Directory.CreateDirectory(externalServicesPath);

        // Generate service client
        var clientContent = GenerateServiceClient(config, service);
        await File.WriteAllTextAsync(
            Path.Combine(externalServicesPath, $"{service.Name}Client.cs"),
            clientContent);
    }

    private async Task GenerateServiceInterfaceAsync(GenerationContext context, ExternalServiceConfiguration service)
    {
        var config = context.Configuration;
        var applicationPath = context.GetApplicationProjectPath();

        // Create external services directory in Application layer
        var externalServicesPath = Path.Combine(applicationPath, "ExternalServices");
        Directory.CreateDirectory(externalServicesPath);

        // Generate service interface
        var interfaceContent = GenerateServiceInterface(config, service);
        await File.WriteAllTextAsync(
            Path.Combine(externalServicesPath, $"I{service.Name}Service.cs"),
            interfaceContent);
    }

    private async Task GenerateResilienceInfrastructureAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var infrastructurePath = context.GetInfrastructureProjectPath();

        // Create resilience directory
        var resiliencePath = Path.Combine(infrastructurePath, "ExternalServices", "Resilience");
        Directory.CreateDirectory(resiliencePath);

        // Generate resilience policies
        var resiliencePoliciesContent = GenerateResiliencePolicies(config);
        await File.WriteAllTextAsync(
            Path.Combine(resiliencePath, "ResiliencePolicies.cs"),
            resiliencePoliciesContent);

        // Generate HTTP client factory
        var httpClientFactoryContent = GenerateHttpClientFactory(config);
        await File.WriteAllTextAsync(
            Path.Combine(resiliencePath, "HttpClientFactory.cs"),
            httpClientFactoryContent);

        // Generate authentication handlers
        var authHandlersContent = GenerateAuthenticationHandlers(config);
        await File.WriteAllTextAsync(
            Path.Combine(resiliencePath, "AuthenticationHandlers.cs"),
            authHandlersContent);
    }

    private async Task GenerateServiceRegistryAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var infrastructurePath = context.GetInfrastructureProjectPath();

        // Create service registry directory
        var serviceRegistryPath = Path.Combine(infrastructurePath, "ExternalServices", "Registry");
        Directory.CreateDirectory(serviceRegistryPath);

        // Generate service registry
        var serviceRegistryContent = GenerateServiceRegistry(config);
        await File.WriteAllTextAsync(
            Path.Combine(serviceRegistryPath, "ServiceRegistry.cs"),
            serviceRegistryContent);

        // Generate service discovery
        var serviceDiscoveryContent = GenerateServiceDiscovery(config);
        await File.WriteAllTextAsync(
            Path.Combine(serviceRegistryPath, "ServiceDiscovery.cs"),
            serviceDiscoveryContent);
    }

    private async Task GenerateExternalServicesExtensionsAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var infrastructurePath = context.GetInfrastructureProjectPath();

        // Generate external services extensions
        var extensionsContent = GenerateExternalServicesExtensions(config);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "Extensions", "ExternalServicesExtensions.cs"),
            extensionsContent);
    }

    private string GenerateServiceClient(TemplateConfiguration config, ExternalServiceConfiguration service)
    {
        var operations = string.Join("\n\n", service.Operations.Select(op => GenerateOperation(service, op)));

        return $@"using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using {config.Namespace}.Application.ExternalServices;

namespace {config.Namespace}.Infrastructure.ExternalServices.Clients;

public class {service.Name}Client : I{service.Name}Service
{{
    private readonly HttpClient _httpClient;
    private readonly ILogger<{service.Name}Client> _logger;
    private readonly {service.Name}Settings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public {service.Name}Client(
        HttpClient httpClient,
        ILogger<{service.Name}Client> logger,
        IOptions<{service.Name}Settings> settings)
    {{
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
        _jsonOptions = new JsonSerializerOptions
        {{
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        }};
    }}

{operations}

    private async Task<T?> SendRequestAsync<T>(
        HttpMethod method,
        string endpoint,
        object? content = null,
        CancellationToken cancellationToken = default)
    {{
        try
        {{
            using var request = new HttpRequestMessage(method, endpoint);

            if (content != null)
            {{
                var json = JsonSerializer.Serialize(content, _jsonOptions);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, ""application/json"");
            }}

            _logger.LogDebug(""Sending {{Method}} request to {{Endpoint}}"", method, endpoint);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {{
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                
                if (typeof(T) == typeof(string))
                {{
                    return (T)(object)responseContent;
                }}
                
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }}

            _logger.LogWarning(""Request failed with status {{StatusCode}}: {{ReasonPhrase}}"", 
                response.StatusCode, response.ReasonPhrase);
            
            return default;
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""Error sending request to {{Endpoint}}"", endpoint);
            throw;
        }}
    }}
}}

public class {service.Name}Settings
{{
    public string BaseUrl {{ get; set; }} = ""{service.BaseUrl}"";
    public string ApiKey {{ get; set; }} = string.Empty;
    public int TimeoutSeconds {{ get; set; }} = 30;
    public bool EnableRetry {{ get; set; }} = true;
    public int MaxRetryAttempts {{ get; set; }} = 3;
}}";
    }

    private string GenerateOperation(ExternalServiceConfiguration service, string operation)
    {
        return $@"    public async Task<{operation}Response?> {operation}Async(
        {operation}Request request,
        CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Executing {{Operation}} for {service.Name}"", nameof({operation}));

        var response = await SendRequestAsync<{operation}Response>(
            HttpMethod.Post,
            ""/api/{operation.ToLowerInvariant()}"",
            request,
            cancellationToken);

        _logger.LogInformation(""Completed {{Operation}} for {service.Name}"", nameof({operation}));
        
        return response;
    }}";
    }

    private string GenerateServiceInterface(TemplateConfiguration config, ExternalServiceConfiguration service)
    {
        var operations = string.Join("\n", service.Operations.Select(op => 
            $"    Task<{op}Response?> {op}Async({op}Request request, CancellationToken cancellationToken = default);"));

        return $@"namespace {config.Namespace}.Application.ExternalServices;

/// <summary>
/// Service interface for {service.Name} external service
/// </summary>
public interface I{service.Name}Service
{{
{operations}
}}

// Request/Response DTOs for {service.Name}
{string.Join("\n\n", service.Operations.SelectMany(op => new[]
{
    GenerateRequestDto(op),
    GenerateResponseDto(op)
}))}";
    }

    private string GenerateRequestDto(string operation)
    {
        return $@"public class {operation}Request
{{
    // TODO: Add properties specific to {operation}
    public string RequestId {{ get; set; }} = Guid.NewGuid().ToString();
    public DateTime Timestamp {{ get; set; }} = DateTime.UtcNow;
}}";
    }

    private string GenerateResponseDto(string operation)
    {
        return $@"public class {operation}Response
{{
    public bool Success {{ get; set; }}
    public string? Message {{ get; set; }}
    public string? ErrorCode {{ get; set; }}
    public DateTime Timestamp {{ get; set; }} = DateTime.UtcNow;
    
    // TODO: Add properties specific to {operation} response
}}";
    }

    private string GenerateResiliencePolicies(TemplateConfiguration config)
    {
        var resilienceConfig = config.Features?.ExternalServices?.Resilience;

        return $@"using Polly;
using Polly.Extensions.Http;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Microsoft.Extensions.Logging;

namespace {config.Namespace}.Infrastructure.ExternalServices.Resilience;

public static class ResiliencePolicies
{{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
    {{
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: {resilienceConfig?.Retry?.MaxAttempts ?? 3},
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {{
                    logger.LogWarning(""Retry {{RetryCount}} for {{OperationKey}} in {{Delay}}ms"",
                        retryCount, context.OperationKey, timespan.TotalMilliseconds);
                }});
    }}

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
    {{
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: {resilienceConfig?.CircuitBreaker?.FailureThreshold ?? 5},
                durationOfBreak: TimeSpan.FromSeconds({resilienceConfig?.CircuitBreaker?.RecoveryTimeMs / 1000 ?? 60}),
                onBreak: (exception, duration) =>
                {{
                    logger.LogWarning(""Circuit breaker opened for {{Duration}}s"", duration.TotalSeconds);
                }},
                onReset: () =>
                {{
                    logger.LogInformation(""Circuit breaker reset"");
                }});
    }}

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {{
        return Policy.TimeoutAsync<HttpResponseMessage>({resilienceConfig?.Timeout?.DefaultTimeoutMs / 1000 ?? 30});
    }}

    public static IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy()
    {{
        return Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: {resilienceConfig?.Bulkhead?.MaxConcurrency ?? 10},
            maxQueuingActions: {resilienceConfig?.Bulkhead?.QueueCapacity ?? 100});
    }}

    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy(ILogger logger)
    {{
        var retryPolicy = GetRetryPolicy(logger);
        var circuitBreakerPolicy = GetCircuitBreakerPolicy(logger);
        var timeoutPolicy = GetTimeoutPolicy();

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
    }}
}}";
    }

    private string GenerateHttpClientFactory(TemplateConfiguration config)
    {
        return $@"using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using {config.Namespace}.Infrastructure.ExternalServices.Resilience;

namespace {config.Namespace}.Infrastructure.ExternalServices;

public class ResilientHttpClientFactory
{{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ResilientHttpClientFactory> _logger;

    public ResilientHttpClientFactory(
        IServiceProvider serviceProvider,
        ILogger<ResilientHttpClientFactory> logger)
    {{
        _serviceProvider = serviceProvider;
        _logger = logger;
    }}

    public HttpClient CreateClient(string name)
    {{
        var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient(name);

        _logger.LogDebug(""Created HTTP client for {{ServiceName}}"", name);
        
        return client;
    }}

    public HttpClient CreateResilientClient(string name)
    {{
        var client = CreateClient(name);
        
        // Additional resilience configuration can be added here
        // if not handled by Polly policies in DI registration
        
        return client;
    }}
}}";
    }

    private string GenerateAuthenticationHandlers(TemplateConfiguration config)
    {
        return $@"using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace {config.Namespace}.Infrastructure.ExternalServices.Authentication;

public class ApiKeyAuthenticationHandler : DelegatingHandler
{{
    private readonly ApiKeySettings _settings;

    public ApiKeyAuthenticationHandler(IOptions<ApiKeySettings> settings)
    {{
        _settings = settings.Value;
    }}

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {{
        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
        {{
            request.Headers.Add(_settings.HeaderName, _settings.ApiKey);
        }}

        return await base.SendAsync(request, cancellationToken);
    }}
}}

public class BearerTokenAuthenticationHandler : DelegatingHandler
{{
    private readonly BearerTokenSettings _settings;

    public BearerTokenAuthenticationHandler(IOptions<BearerTokenSettings> settings)
    {{
        _settings = settings.Value;
    }}

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {{
        if (!string.IsNullOrWhiteSpace(_settings.Token))
        {{
            request.Headers.Authorization = new AuthenticationHeaderValue(""Bearer"", _settings.Token);
        }}

        return await base.SendAsync(request, cancellationToken);
    }}
}}

public class ApiKeySettings
{{
    public string ApiKey {{ get; set; }} = string.Empty;
    public string HeaderName {{ get; set; }} = ""X-API-Key"";
}}

public class BearerTokenSettings
{{
    public string Token {{ get; set; }} = string.Empty;
}}";
    }

    private string GenerateServiceRegistry(TemplateConfiguration config)
    {
        return $@"using Microsoft.Extensions.Options;

namespace {config.Namespace}.Infrastructure.ExternalServices.Registry;

public interface IServiceRegistry
{{
    Task<ServiceEndpoint?> GetServiceEndpointAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<List<ServiceEndpoint>> GetAllServiceEndpointsAsync(string serviceName, CancellationToken cancellationToken = default);
    Task RegisterServiceAsync(ServiceEndpoint endpoint, CancellationToken cancellationToken = default);
    Task<bool> IsServiceHealthyAsync(string serviceName, CancellationToken cancellationToken = default);
}}

public class ServiceRegistry : IServiceRegistry
{{
    private readonly ServiceRegistrySettings _settings;
    private readonly Dictionary<string, List<ServiceEndpoint>> _services;

    public ServiceRegistry(IOptions<ServiceRegistrySettings> settings)
    {{
        _settings = settings.Value;
        _services = new Dictionary<string, List<ServiceEndpoint>>();
        
        // Initialize with configured services
        InitializeServices();
    }}

    public Task<ServiceEndpoint?> GetServiceEndpointAsync(string serviceName, CancellationToken cancellationToken = default)
    {{
        if (_services.TryGetValue(serviceName, out var endpoints) && endpoints.Any())
        {{
            // Simple round-robin selection
            var endpoint = endpoints.First();
            return Task.FromResult<ServiceEndpoint?>(endpoint);
        }}

        return Task.FromResult<ServiceEndpoint?>(null);
    }}

    public Task<List<ServiceEndpoint>> GetAllServiceEndpointsAsync(string serviceName, CancellationToken cancellationToken = default)
    {{
        if (_services.TryGetValue(serviceName, out var endpoints))
        {{
            return Task.FromResult(endpoints);
        }}

        return Task.FromResult(new List<ServiceEndpoint>());
    }}

    public Task RegisterServiceAsync(ServiceEndpoint endpoint, CancellationToken cancellationToken = default)
    {{
        if (!_services.ContainsKey(endpoint.ServiceName))
        {{
            _services[endpoint.ServiceName] = new List<ServiceEndpoint>();
        }}

        _services[endpoint.ServiceName].Add(endpoint);
        return Task.CompletedTask;
    }}

    public async Task<bool> IsServiceHealthyAsync(string serviceName, CancellationToken cancellationToken = default)
    {{
        var endpoint = await GetServiceEndpointAsync(serviceName, cancellationToken);
        if (endpoint == null) return false;

        try
        {{
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            
            var response = await httpClient.GetAsync($""{{endpoint.BaseUrl}}/health"", cancellationToken);
            return response.IsSuccessStatusCode;
        }}
        catch
        {{
            return false;
        }}
    }}

    private void InitializeServices()
    {{
        foreach (var service in _settings.Services)
        {{
            var endpoint = new ServiceEndpoint
            {{
                ServiceName = service.Key,
                BaseUrl = service.Value,
                IsHealthy = true,
                LastHealthCheck = DateTime.UtcNow
            }};

            RegisterServiceAsync(endpoint).Wait();
        }}
    }}
}}

public class ServiceEndpoint
{{
    public string ServiceName {{ get; set; }} = string.Empty;
    public string BaseUrl {{ get; set; }} = string.Empty;
    public bool IsHealthy {{ get; set; }} = true;
    public DateTime LastHealthCheck {{ get; set; }} = DateTime.UtcNow;
    public Dictionary<string, string> Metadata {{ get; set; }} = new();
}}

public class ServiceRegistrySettings
{{
    public Dictionary<string, string> Services {{ get; set; }} = new();
    public int HealthCheckIntervalSeconds {{ get; set; }} = 30;
}}";
    }

    private string GenerateServiceDiscovery(TemplateConfiguration config)
    {
        return $@"using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace {config.Namespace}.Infrastructure.ExternalServices.Registry;

public class ServiceDiscoveryService : BackgroundService
{{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceDiscoveryService> _logger;
    private readonly TimeSpan _healthCheckInterval = TimeSpan.FromSeconds(30);

    public ServiceDiscoveryService(
        IServiceProvider serviceProvider,
        ILogger<ServiceDiscoveryService> logger)
    {{
        _serviceProvider = serviceProvider;
        _logger = logger;
    }}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {{
        _logger.LogInformation(""Service discovery started"");

        while (!stoppingToken.IsCancellationRequested)
        {{
            try
            {{
                await PerformHealthChecksAsync(stoppingToken);
            }}
            catch (Exception ex)
            {{
                _logger.LogError(ex, ""Error during service health checks"");
            }}

            await Task.Delay(_healthCheckInterval, stoppingToken);
        }}

        _logger.LogInformation(""Service discovery stopped"");
    }}

    private async Task PerformHealthChecksAsync(CancellationToken cancellationToken)
    {{
        using var scope = _serviceProvider.CreateScope();
        var serviceRegistry = scope.ServiceProvider.GetRequiredService<IServiceRegistry>();

        // Get all registered services and check their health
        // This is a simplified implementation
        // In a real scenario, you'd iterate through all registered services
        
        var services = new[] {{ ""PaymentGateway"", ""NotificationService"", ""FraudDetectionService"" }};
        
        foreach (var serviceName in services)
        {{
            try
            {{
                var isHealthy = await serviceRegistry.IsServiceHealthyAsync(serviceName, cancellationToken);
                _logger.LogDebug(""Service {{ServiceName}} health status: {{IsHealthy}}"", serviceName, isHealthy);
            }}
            catch (Exception ex)
            {{
                _logger.LogWarning(ex, ""Failed to check health for service {{ServiceName}}"", serviceName);
            }}
        }}
    }}
}}";
    }

    private string GenerateExternalServicesExtensions(TemplateConfiguration config)
    {
        var externalServicesConfig = config.Features?.ExternalServices;
        var serviceRegistrations = string.Empty;

        if (externalServicesConfig?.Services != null)
        {
            serviceRegistrations = string.Join("\n\n", externalServicesConfig.Services.Select(service => 
                GenerateServiceRegistration(service)));
        }

        return $@"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using {config.Namespace}.Application.ExternalServices;
using {config.Namespace}.Infrastructure.ExternalServices;
using {config.Namespace}.Infrastructure.ExternalServices.Clients;
using {config.Namespace}.Infrastructure.ExternalServices.Authentication;
using {config.Namespace}.Infrastructure.ExternalServices.Registry;
using {config.Namespace}.Infrastructure.ExternalServices.Resilience;

namespace {config.Namespace}.Infrastructure.Extensions;

public static class ExternalServicesExtensions
{{
    public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {{
        // Register service registry
        services.Configure<ServiceRegistrySettings>(configuration.GetSection(""ServiceRegistry""));
        services.AddSingleton<IServiceRegistry, ServiceRegistry>();
        services.AddHostedService<ServiceDiscoveryService>();

        // Register HTTP client factory
        services.AddSingleton<ResilientHttpClientFactory>();

        // Register external service clients
        RegisterExternalServiceClients(services, configuration);

        // Add health checks for external services
        AddExternalServiceHealthChecks(services, configuration);

        return services;
    }}

    private static void RegisterExternalServiceClients(IServiceCollection services, IConfiguration configuration)
    {{
{serviceRegistrations}
    }}

    private static void AddExternalServiceHealthChecks(IServiceCollection services, IConfiguration configuration)
    {{
        var healthChecksBuilder = services.AddHealthChecks();

        // Add health checks for each external service
{string.Join("\n", externalServicesConfig?.Services?.Select(service => 
    $"        healthChecksBuilder.AddUrlGroup(new Uri(configuration[\"{service.Name}:BaseUrl\"] ?? \"{service.BaseUrl}\"), \"{service.Name.ToLowerInvariant()}\");") ?? new string[0])}
    }}
}}";
    }

    private string GenerateServiceRegistration(ExternalServiceConfiguration service)
    {
        return $@"        // Register {service.Name}
        services.Configure<{service.Name}Settings>(configuration.GetSection(""{service.Name}""));
        
        services.AddHttpClient<{service.Name}Client>(client =>
        {{
            client.BaseAddress = new Uri(configuration[""{service.Name}:BaseUrl""] ?? ""{service.BaseUrl}"");
            client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>(""{service.Name}:TimeoutSeconds"", 30));
        }})
        .AddPolicyHandler((serviceProvider, request) =>
        {{
            var logger = serviceProvider.GetRequiredService<ILogger<{service.Name}Client>>();
            return ResiliencePolicies.GetCombinedPolicy(logger);
        }});

        services.AddScoped<I{service.Name}Service, {service.Name}Client>();";
    }
} 