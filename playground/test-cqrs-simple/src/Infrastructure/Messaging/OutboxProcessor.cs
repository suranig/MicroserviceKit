using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleService.Infrastructure.Messaging.Publishers;

namespace SimpleService.Infrastructure.Messaging;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);

    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxEventsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox events");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox processor stopped");
    }

    private async Task ProcessOutboxEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IDomainEventPublisher>();

        var pendingEvents = await outboxRepository.GetUnprocessedEventsAsync(100, cancellationToken);

        if (pendingEvents.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox events", pendingEvents.Count);

        foreach (var outboxEvent in pendingEvents)
        {
            try
            {
                // TODO: Deserialize and publish the event
                // This would require a more sophisticated event serialization strategy
                
                await outboxRepository.MarkAsProcessedAsync(outboxEvent.Id, cancellationToken);
                
                _logger.LogDebug("Processed outbox event {EventId} of type {EventType}", 
                    outboxEvent.Id, outboxEvent.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox event {EventId}", outboxEvent.Id);
                await outboxRepository.MarkAsFailedAsync(outboxEvent.Id, ex.Message, cancellationToken);
            }
        }
    }
}