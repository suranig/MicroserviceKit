using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.Messaging;

public class MessagingModule : ITemplateModule
{
    public string Name => "Messaging";
    public string Description => "Generates event-driven messaging with RabbitMQ, domain events, and event handlers";

    public bool IsEnabled(TemplateConfiguration config)
    {
        return config.Features?.Messaging?.Enabled == true;
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var messagingConfig = config.Features?.Messaging;

        if (messagingConfig?.Provider?.ToLowerInvariant() == "rabbitmq")
        {
            await GenerateRabbitMQInfrastructureAsync(context);
        }

        // Generate domain events for each aggregate
        if (config.Domain?.Aggregates != null)
        {
            foreach (var aggregate in config.Domain.Aggregates)
            {
                await GenerateDomainEventsAsync(context, aggregate);
                await GenerateEventHandlersAsync(context, aggregate);
            }
        }

        // Generate messaging infrastructure
        await GenerateEventDispatcherAsync(context);
        await GenerateOutboxPatternAsync(context);
        await GenerateMessagingExtensionsAsync(context);
    }

    private async Task GenerateRabbitMQInfrastructureAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var infrastructurePath = context.GetInfrastructureProjectPath();

        // Create messaging directories
        Directory.CreateDirectory(Path.Combine(infrastructurePath, "Messaging"));
        Directory.CreateDirectory(Path.Combine(infrastructurePath, "Messaging", "Configuration"));
        Directory.CreateDirectory(Path.Combine(infrastructurePath, "Messaging", "Publishers"));
        Directory.CreateDirectory(Path.Combine(infrastructurePath, "Messaging", "Consumers"));

        // Generate RabbitMQ configuration
        var rabbitMQConfig = GenerateRabbitMQConfiguration(config);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "Messaging", "Configuration", "RabbitMQConfiguration.cs"),
            rabbitMQConfig);

        // Generate event publisher
        var eventPublisher = GenerateEventPublisher(config);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "Messaging", "Publishers", "DomainEventPublisher.cs"),
            eventPublisher);

        // Generate outbox repository
        var outboxRepository = GenerateOutboxRepository(config);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "Messaging", "Publishers", "OutboxRepository.cs"),
            outboxRepository);
    }

    private async Task GenerateDomainEventsAsync(GenerationContext context, AggregateConfiguration aggregate)
    {
        var config = context.Configuration;
        var domainPath = context.GetDomainProjectPath();

        // Generate additional domain events for the aggregate
        var events = new[]
        {
            $"{aggregate.Name}UpdatedEvent",
            $"{aggregate.Name}DeletedEvent"
        };

        foreach (var eventName in events)
        {
            var eventContent = GenerateDomainEvent(config, aggregate, eventName);
            await File.WriteAllTextAsync(
                Path.Combine(domainPath, "Events", $"{eventName}.cs"),
                eventContent);
        }

        // Generate integration events
        var integrationEventContent = GenerateIntegrationEvent(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(domainPath, "Events", $"{aggregate.Name}IntegrationEvent.cs"),
            integrationEventContent);
    }

    private async Task GenerateEventHandlersAsync(GenerationContext context, AggregateConfiguration aggregate)
    {
        var config = context.Configuration;
        var applicationPath = context.GetApplicationProjectPath();

        // Create event handlers directory
        var eventHandlersPath = Path.Combine(applicationPath, aggregate.Name, "EventHandlers");
        Directory.CreateDirectory(eventHandlersPath);

        // Generate domain event handlers
        var events = new[]
        {
            $"{aggregate.Name}CreatedEvent",
            $"{aggregate.Name}UpdatedEvent", 
            $"{aggregate.Name}DeletedEvent"
        };

        foreach (var eventName in events)
        {
            var handlerContent = GenerateEventHandler(config, aggregate, eventName);
            await File.WriteAllTextAsync(
                Path.Combine(eventHandlersPath, $"{eventName}Handler.cs"),
                handlerContent);
        }

        // Generate read model updater
        var readModelUpdater = GenerateReadModelUpdater(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(eventHandlersPath, $"{aggregate.Name}ReadModelUpdater.cs"),
            readModelUpdater);
    }

    private async Task GenerateEventDispatcherAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var applicationPath = context.GetApplicationProjectPath();

        // Create common directory
        Directory.CreateDirectory(Path.Combine(applicationPath, "Common", "Events"));

        // Generate event dispatcher interface
        var eventDispatcherInterface = GenerateEventDispatcherInterface(config);
        await File.WriteAllTextAsync(
            Path.Combine(applicationPath, "Common", "Events", "IEventDispatcher.cs"),
            eventDispatcherInterface);

        // Generate event dispatcher implementation
        var eventDispatcher = GenerateEventDispatcher(config);
        await File.WriteAllTextAsync(
            Path.Combine(applicationPath, "Common", "Events", "EventDispatcher.cs"),
            eventDispatcher);

        // Generate event handler interface
        var eventHandlerInterface = GenerateEventHandlerInterface(config);
        await File.WriteAllTextAsync(
            Path.Combine(applicationPath, "Common", "Events", "IEventHandler.cs"),
            eventHandlerInterface);
    }

    private async Task GenerateOutboxPatternAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var infrastructurePath = context.GetInfrastructureProjectPath();

        // Generate outbox entity
        var outboxEntity = GenerateOutboxEntity(config);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "Messaging", "OutboxEvent.cs"),
            outboxEntity);

        // Generate outbox processor
        var outboxProcessor = GenerateOutboxProcessor(config);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "Messaging", "OutboxProcessor.cs"),
            outboxProcessor);
    }

    private async Task GenerateMessagingExtensionsAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var infrastructurePath = context.GetInfrastructureProjectPath();

        // Generate messaging extensions
        var messagingExtensions = GenerateMessagingExtensions(config);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "Extensions", "MessagingExtensions.cs"),
            messagingExtensions);
    }

    private string GenerateRabbitMQConfiguration(TemplateConfiguration config)
    {
        return $@"using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace {config.Namespace}.Infrastructure.Messaging.Configuration;

public class RabbitMQConfiguration
{{
    public string HostName {{ get; set; }} = ""localhost"";
    public int Port {{ get; set; }} = 5672;
    public string VirtualHost {{ get; set; }} = ""/"";
    public string UserName {{ get; set; }} = ""guest"";
    public string Password {{ get; set; }} = ""guest"";
    public string ExchangeName {{ get; set; }} = ""{config.MicroserviceName.ToLowerInvariant()}.events"";
    public bool Durable {{ get; set; }} = true;
    public bool AutoDelete {{ get; set; }} = false;
}}

public static class RabbitMQServiceExtensions
{{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {{
        var rabbitMQConfig = configuration.GetSection(""RabbitMQ"").Get<RabbitMQConfiguration>() 
            ?? new RabbitMQConfiguration();

        services.AddSingleton(rabbitMQConfig);

        services.AddSingleton<IConnectionFactory>(provider =>
        {{
            var config = provider.GetRequiredService<RabbitMQConfiguration>();
            return new ConnectionFactory
            {{
                HostName = config.HostName,
                Port = config.Port,
                VirtualHost = config.VirtualHost,
                UserName = config.UserName,
                Password = config.Password,
                DispatchConsumersAsync = true
            }};
        }});

        services.AddSingleton<IConnection>(provider =>
        {{
            var factory = provider.GetRequiredService<IConnectionFactory>();
            var logger = provider.GetRequiredService<ILogger<IConnection>>();
            
            try
            {{
                var connection = factory.CreateConnection(""{config.MicroserviceName}"");
                logger.LogInformation(""Connected to RabbitMQ"");
                return connection;
            }}
            catch (Exception ex)
            {{
                logger.LogError(ex, ""Failed to connect to RabbitMQ"");
                throw;
            }}
        }});

        services.AddScoped<IModel>(provider =>
        {{
            var connection = provider.GetRequiredService<IConnection>();
            var channel = connection.CreateModel();
            
            var config = provider.GetRequiredService<RabbitMQConfiguration>();
            
            // Declare exchange
            channel.ExchangeDeclare(
                exchange: config.ExchangeName,
                type: ExchangeType.Topic,
                durable: config.Durable,
                autoDelete: config.AutoDelete);
                
            return channel;
        }});

        return services;
    }}
}}";
    }

    private string GenerateEventPublisher(TemplateConfiguration config)
    {
        return $@"using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using {config.Namespace}.Application.Common.Events;
using AggregateKit;

namespace {config.Namespace}.Infrastructure.Messaging.Publishers;

public interface IDomainEventPublisher
{{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : DomainEventBase;
    Task PublishBatchAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default) where T : DomainEventBase;
}}

public class DomainEventPublisher : IDomainEventPublisher
{{
    private readonly IModel _channel;
    private readonly RabbitMQConfiguration _config;
    private readonly ILogger<DomainEventPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DomainEventPublisher(
        IModel channel,
        RabbitMQConfiguration config,
        ILogger<DomainEventPublisher> logger)
    {{
        _channel = channel;
        _config = config;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {{
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        }};
    }}

    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) 
        where T : DomainEventBase
    {{
        try
        {{
            var routingKey = GenerateRoutingKey(typeof(T).Name);
            var message = JsonSerializer.Serialize(domainEvent, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = typeof(T).Name;
            properties.Headers = new Dictionary<string, object>
            {{
                [""EventType""] = typeof(T).Name,
                [""EventVersion""] = ""1.0"",
                [""Source""] = ""{config.MicroserviceName}"",
                [""CorrelationId""] = domainEvent.EventId.ToString()
            }};

            _channel.BasicPublish(
                exchange: _config.ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(""Published domain event {{EventType}} with ID {{EventId}}"", 
                typeof(T).Name, domainEvent.EventId);

            await Task.CompletedTask;
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""Failed to publish domain event {{EventType}}"", typeof(T).Name);
            throw;
        }}
    }}

    public async Task PublishBatchAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default) 
        where T : DomainEventBase
    {{
        var batch = _channel.CreateBasicPublishBatch();
        
        foreach (var domainEvent in domainEvents)
        {{
            var routingKey = GenerateRoutingKey(typeof(T).Name);
            var message = JsonSerializer.Serialize(domainEvent, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = typeof(T).Name;

            batch.Add(_config.ExchangeName, routingKey, false, properties, body);
        }}

        batch.Publish();
        _logger.LogInformation(""Published batch of {{Count}} domain events"", domainEvents.Count());
        
        await Task.CompletedTask;
    }}

    private string GenerateRoutingKey(string eventType)
    {{
        // Convert PascalCase to kebab-case for routing key
        return eventType.ToLowerInvariant().Replace(""event"", """");
    }}
}}";
    }

    private string GenerateDomainEvent(TemplateConfiguration config, AggregateConfiguration aggregate, string eventName)
    {
        var eventType = eventName.Replace($"{aggregate.Name}", "").Replace("Event", "");
        var parameters = eventType.ToLowerInvariant() switch
        {
            "created" => $"Guid {aggregate.Name}Id, {GenerateEventParameters(aggregate.Properties)}",
            "updated" => $"Guid {aggregate.Name}Id, {GenerateEventParameters(aggregate.Properties)}",
            "deleted" => $"Guid {aggregate.Name}Id",
            _ => $"Guid {aggregate.Name}Id"
        };

        return $@"using AggregateKit;

namespace {config.Namespace}.Domain.Events;

public record {eventName}({parameters}) : DomainEventBase;";
    }

    private string GenerateIntegrationEvent(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        return $@"using AggregateKit;

namespace {config.Namespace}.Domain.Events;

/// <summary>
/// Integration event for {aggregate.Name} - published to external services
/// </summary>
public record {aggregate.Name}IntegrationEvent(
    Guid {aggregate.Name}Id,
    string EventType,
    {GenerateEventParameters(aggregate.Properties)},
    DateTime OccurredAt
) : DomainEventBase;";
    }

    private string GenerateEventHandler(TemplateConfiguration config, AggregateConfiguration aggregate, string eventName)
    {
        var handlerName = $"{eventName}Handler";
        
        return $@"using Microsoft.Extensions.Logging;
using {config.Namespace}.Application.Common.Events;
using {config.Namespace}.Domain.Events;

namespace {config.Namespace}.Application.{aggregate.Name}.EventHandlers;

public class {handlerName} : IEventHandler<{eventName}>
{{
    private readonly ILogger<{handlerName}> _logger;

    public {handlerName}(ILogger<{handlerName}> logger)
    {{
        _logger = logger;
    }}

    public async Task Handle({eventName} domainEvent, CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Handling {{EventType}} for {aggregate.Name} {{AggregateId}}"", 
            nameof({eventName}), domainEvent.{aggregate.Name}Id);

        try
        {{
            // TODO: Implement business logic for {eventName}
            // Examples:
            // - Update read models
            // - Send notifications
            // - Trigger workflows
            // - Update caches
            
            await Task.CompletedTask;
            
            _logger.LogInformation(""Successfully handled {{EventType}} for {aggregate.Name} {{AggregateId}}"", 
                nameof({eventName}), domainEvent.{aggregate.Name}Id);
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""Failed to handle {{EventType}} for {aggregate.Name} {{AggregateId}}"", 
                nameof({eventName}), domainEvent.{aggregate.Name}Id);
            throw;
        }}
    }}
}}";
    }

    private string GenerateReadModelUpdater(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        return $@"using Microsoft.Extensions.Logging;
using {config.Namespace}.Application.Common.Events;
using {config.Namespace}.Application.{aggregate.Name}.ReadModels;
using {config.Namespace}.Domain.Events;

namespace {config.Namespace}.Application.{aggregate.Name}.EventHandlers;

/// <summary>
/// Updates read models when {aggregate.Name} domain events occur
/// </summary>
public class {aggregate.Name}ReadModelUpdater : 
    IEventHandler<{aggregate.Name}CreatedEvent>,
    IEventHandler<{aggregate.Name}UpdatedEvent>,
    IEventHandler<{aggregate.Name}DeletedEvent>
{{
    private readonly ILogger<{aggregate.Name}ReadModelUpdater> _logger;
    // TODO: Add read model repository dependency

    public {aggregate.Name}ReadModelUpdater(ILogger<{aggregate.Name}ReadModelUpdater> logger)
    {{
        _logger = logger;
    }}

    public async Task Handle({aggregate.Name}CreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Updating read model for created {aggregate.Name} {{AggregateId}}"", 
            domainEvent.{aggregate.Name}Id);

        // TODO: Create read model entry
        var readModel = new {aggregate.Name}ReadModel
        {{
            Id = domainEvent.{aggregate.Name}Id,
{string.Join(",\n", aggregate.Properties.Select(p => $"            {p.Name} = domainEvent.{p.Name}"))}
        }};

        // TODO: Save to read model store (MongoDB, etc.)
        
        await Task.CompletedTask;
    }}

    public async Task Handle({aggregate.Name}UpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Updating read model for updated {aggregate.Name} {{AggregateId}}"", 
            domainEvent.{aggregate.Name}Id);

        // TODO: Update existing read model entry
        
        await Task.CompletedTask;
    }}

    public async Task Handle({aggregate.Name}DeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Updating read model for deleted {aggregate.Name} {{AggregateId}}"", 
            domainEvent.{aggregate.Name}Id);

        // TODO: Remove read model entry
        
        await Task.CompletedTask;
    }}
}}";
    }

    private string GenerateEventDispatcherInterface(TemplateConfiguration config)
    {
        return $@"using AggregateKit;

namespace {config.Namespace}.Application.Common.Events;

public interface IEventDispatcher
{{
    Task DispatchAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : DomainEventBase;
    Task DispatchAsync(IEnumerable<DomainEventBase> domainEvents, CancellationToken cancellationToken = default);
}}";
    }

    private string GenerateEventDispatcher(TemplateConfiguration config)
    {
        return $@"using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AggregateKit;

namespace {config.Namespace}.Application.Common.Events;

public class EventDispatcher : IEventDispatcher
{{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventDispatcher> _logger;

    public EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> logger)
    {{
        _serviceProvider = serviceProvider;
        _logger = logger;
    }}

    public async Task DispatchAsync<T>(T domainEvent, CancellationToken cancellationToken = default) 
        where T : DomainEventBase
    {{
        _logger.LogDebug(""Dispatching domain event {{EventType}} with ID {{EventId}}"", 
            typeof(T).Name, domainEvent.EventId);

        var handlers = _serviceProvider.GetServices<IEventHandler<T>>();
        
        var tasks = handlers.Select(handler => 
            ExecuteHandlerSafely(handler, domainEvent, cancellationToken));
            
        await Task.WhenAll(tasks);
        
        _logger.LogDebug(""Completed dispatching domain event {{EventType}} to {{HandlerCount}} handlers"", 
            typeof(T).Name, handlers.Count());
    }}

    public async Task DispatchAsync(IEnumerable<DomainEventBase> domainEvents, CancellationToken cancellationToken = default)
    {{
        var tasks = domainEvents.Select(domainEvent => 
            DispatchSingleEvent(domainEvent, cancellationToken));
            
        await Task.WhenAll(tasks);
    }}

    private async Task DispatchSingleEvent(DomainEventBase domainEvent, CancellationToken cancellationToken)
    {{
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType);

        var tasks = handlers.Select(handler => 
            ExecuteHandlerSafely(handler, domainEvent, cancellationToken));
            
        await Task.WhenAll(tasks);
    }}

    private async Task ExecuteHandlerSafely(object handler, DomainEventBase domainEvent, CancellationToken cancellationToken)
    {{
        try
        {{
            var handleMethod = handler.GetType().GetMethod(""Handle"");
            if (handleMethod != null)
            {{
                var task = (Task)handleMethod.Invoke(handler, new object[] {{ domainEvent, cancellationToken }})!;
                await task;
            }}
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""Error executing event handler {{HandlerType}} for event {{EventType}}"", 
                handler.GetType().Name, domainEvent.GetType().Name);
            // Don't rethrow - we want other handlers to continue executing
        }}
    }}
}}";
    }

    private string GenerateEventHandlerInterface(TemplateConfiguration config)
    {
        return $@"using AggregateKit;

namespace {config.Namespace}.Application.Common.Events;

public interface IEventHandler<in T> where T : DomainEventBase
{{
    Task Handle(T domainEvent, CancellationToken cancellationToken = default);
}}";
    }

    private string GenerateOutboxEntity(TemplateConfiguration config)
    {
        return $@"using System.ComponentModel.DataAnnotations;

namespace {config.Namespace}.Infrastructure.Messaging;

public class OutboxEvent
{{
    [Key]
    public Guid Id {{ get; set; }} = Guid.NewGuid();
    
    [Required]
    [MaxLength(255)]
    public string EventType {{ get; set; }} = string.Empty;
    
    [Required]
    public string EventData {{ get; set; }} = string.Empty;
    
    [Required]
    public DateTime CreatedAt {{ get; set; }} = DateTime.UtcNow;
    
    public DateTime? ProcessedAt {{ get; set; }}
    
    public bool IsProcessed {{ get; set; }} = false;
    
    public int RetryCount {{ get; set; }} = 0;
    
    public string? ErrorMessage {{ get; set; }}
    
    [MaxLength(100)]
    public string? CorrelationId {{ get; set; }}
}}";
    }

    private string GenerateOutboxRepository(TemplateConfiguration config)
    {
        return $@"using Microsoft.EntityFrameworkCore;
using {config.Namespace}.Infrastructure.Persistence;

namespace {config.Namespace}.Infrastructure.Messaging.Publishers;

public interface IOutboxRepository
{{
    Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);
    Task<List<OutboxEvent>> GetUnprocessedEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid eventId, string errorMessage, CancellationToken cancellationToken = default);
    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
}}

public class OutboxRepository : IOutboxRepository
{{
    private readonly ApplicationDbContext _context;

    public OutboxRepository(ApplicationDbContext context)
    {{
        _context = context;
    }}

    public async Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default)
    {{
        _context.OutboxEvents.Add(outboxEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }}

    public async Task<List<OutboxEvent>> GetUnprocessedEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {{
        return await _context.OutboxEvents
            .Where(e => !e.IsProcessed && e.RetryCount < 5)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }}

    public async Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {{
        var outboxEvent = await _context.OutboxEvents.FindAsync(new object[] {{ eventId }}, cancellationToken);
        if (outboxEvent != null)
        {{
            outboxEvent.IsProcessed = true;
            outboxEvent.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }}
    }}

    public async Task MarkAsFailedAsync(Guid eventId, string errorMessage, CancellationToken cancellationToken = default)
    {{
        var outboxEvent = await _context.OutboxEvents.FindAsync(new object[] {{ eventId }}, cancellationToken);
        if (outboxEvent != null)
        {{
            outboxEvent.RetryCount++;
            outboxEvent.ErrorMessage = errorMessage;
            await _context.SaveChangesAsync(cancellationToken);
        }}
    }}

    public async Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
    {{
        return await _context.OutboxEvents
            .CountAsync(e => !e.IsProcessed, cancellationToken);
    }}
}}";
    }

    private string GenerateOutboxProcessor(TemplateConfiguration config)
    {
        return $@"using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using {config.Namespace}.Infrastructure.Messaging.Publishers;

namespace {config.Namespace}.Infrastructure.Messaging;

public class OutboxProcessor : BackgroundService
{{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);

    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
    {{
        _serviceProvider = serviceProvider;
        _logger = logger;
    }}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {{
        _logger.LogInformation(""Outbox processor started"");

        while (!stoppingToken.IsCancellationRequested)
        {{
            try
            {{
                await ProcessOutboxEventsAsync(stoppingToken);
            }}
            catch (Exception ex)
            {{
                _logger.LogError(ex, ""Error processing outbox events"");
            }}

            await Task.Delay(_processingInterval, stoppingToken);
        }}

        _logger.LogInformation(""Outbox processor stopped"");
    }}

    private async Task ProcessOutboxEventsAsync(CancellationToken cancellationToken)
    {{
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IDomainEventPublisher>();

        var pendingEvents = await outboxRepository.GetUnprocessedEventsAsync(100, cancellationToken);

        if (pendingEvents.Count == 0)
        {{
            return;
        }}

        _logger.LogInformation(""Processing {{Count}} outbox events"", pendingEvents.Count);

        foreach (var outboxEvent in pendingEvents)
        {{
            try
            {{
                // TODO: Deserialize and publish the event
                // This would require a more sophisticated event serialization strategy
                
                await outboxRepository.MarkAsProcessedAsync(outboxEvent.Id, cancellationToken);
                
                _logger.LogDebug(""Processed outbox event {{EventId}} of type {{EventType}}"", 
                    outboxEvent.Id, outboxEvent.EventType);
            }}
            catch (Exception ex)
            {{
                _logger.LogError(ex, ""Failed to process outbox event {{EventId}}"", outboxEvent.Id);
                await outboxRepository.MarkAsFailedAsync(outboxEvent.Id, ex.Message, cancellationToken);
            }}
        }}
    }}
}}";
    }

    private string GenerateMessagingExtensions(TemplateConfiguration config)
    {
        return $@"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using {config.Namespace}.Application.Common.Events;
using {config.Namespace}.Infrastructure.Messaging;
using {config.Namespace}.Infrastructure.Messaging.Configuration;
using {config.Namespace}.Infrastructure.Messaging.Publishers;

namespace {config.Namespace}.Infrastructure.Extensions;

public static class MessagingExtensions
{{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {{
        // Add RabbitMQ
        services.AddRabbitMQ(configuration);

        // Add event dispatcher
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        // Add domain event publisher
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        // Add outbox pattern
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddHostedService<OutboxProcessor>();

        // Add event handlers
        RegisterEventHandlers(services);

        return services;
    }}

    private static void RegisterEventHandlers(IServiceCollection services)
    {{
        // Auto-register all event handlers from the application assembly
        var applicationAssembly = typeof({config.Namespace}.Application.AssemblyReference).Assembly;
        
        var eventHandlerTypes = applicationAssembly.GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var handlerType in eventHandlerTypes)
        {{
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

            foreach (var @interface in interfaces)
            {{
                services.AddScoped(@interface, handlerType);
            }}
        }}
    }}
}}";
    }

    private string GenerateEventParameters(List<PropertyConfiguration> properties)
    {
        return string.Join(", ", properties.Select(p => $"{p.Type} {p.Name}"));
    }
} 