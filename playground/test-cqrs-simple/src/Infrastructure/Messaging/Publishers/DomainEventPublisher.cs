using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using SimpleService.Application.Common.Events;
using AggregateKit;

namespace SimpleService.Infrastructure.Messaging.Publishers;

public interface IDomainEventPublisher
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : DomainEventBase;
    Task PublishBatchAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default) where T : DomainEventBase;
}

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IModel _channel;
    private readonly RabbitMQConfiguration _config;
    private readonly ILogger<DomainEventPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DomainEventPublisher(
        IModel channel,
        RabbitMQConfiguration config,
        ILogger<DomainEventPublisher> logger)
    {
        _channel = channel;
        _config = config;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) 
        where T : DomainEventBase
    {
        try
        {
            var routingKey = GenerateRoutingKey(typeof(T).Name);
            var message = JsonSerializer.Serialize(domainEvent, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = typeof(T).Name;
            properties.Headers = new Dictionary<string, object>
            {
                ["EventType"] = typeof(T).Name,
                ["EventVersion"] = "1.0",
                ["Source"] = "SimpleService",
                ["CorrelationId"] = domainEvent.EventId.ToString()
            };

            _channel.BasicPublish(
                exchange: _config.ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published domain event {EventType} with ID {EventId}", 
                typeof(T).Name, domainEvent.EventId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish domain event {EventType}", typeof(T).Name);
            throw;
        }
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default) 
        where T : DomainEventBase
    {
        var batch = _channel.CreateBasicPublishBatch();
        
        foreach (var domainEvent in domainEvents)
        {
            var routingKey = GenerateRoutingKey(typeof(T).Name);
            var message = JsonSerializer.Serialize(domainEvent, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = typeof(T).Name;

            batch.Add(_config.ExchangeName, routingKey, false, properties, body);
        }

        batch.Publish();
        _logger.LogInformation("Published batch of {Count} domain events", domainEvents.Count());
        
        await Task.CompletedTask;
    }

    private string GenerateRoutingKey(string eventType)
    {
        // Convert PascalCase to kebab-case for routing key
        return eventType.ToLowerInvariant().Replace("event", "");
    }
}