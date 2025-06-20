using Microsoft.Extensions.Logging;
using TestService.Application.Common.Events;
using TestService.Domain.Events;

namespace TestService.Application.Product.EventHandlers;

public class ProductCreatedEventHandler : IEventHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling {EventType} for Product {AggregateId}", 
            nameof(ProductCreatedEvent), domainEvent.ProductId);

        try
        {
            // TODO: Implement business logic for ProductCreatedEvent
            // Examples:
            // - Update read models
            // - Send notifications
            // - Trigger workflows
            // - Update caches
            
            await Task.CompletedTask;
            
            _logger.LogInformation("Successfully handled {EventType} for Product {AggregateId}", 
                nameof(ProductCreatedEvent), domainEvent.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle {EventType} for Product {AggregateId}", 
                nameof(ProductCreatedEvent), domainEvent.ProductId);
            throw;
        }
    }
}