using Microsoft.Extensions.Logging;
using TestService.Application.Common.Events;
using TestService.Domain.Events;

namespace TestService.Application.Product.EventHandlers;

public class ProductDeletedEventHandler : IEventHandler<ProductDeletedEvent>
{
    private readonly ILogger<ProductDeletedEventHandler> _logger;

    public ProductDeletedEventHandler(ILogger<ProductDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling {EventType} for Product {AggregateId}", 
            nameof(ProductDeletedEvent), domainEvent.ProductId);

        try
        {
            // TODO: Implement business logic for ProductDeletedEvent
            // Examples:
            // - Update read models
            // - Send notifications
            // - Trigger workflows
            // - Update caches
            
            await Task.CompletedTask;
            
            _logger.LogInformation("Successfully handled {EventType} for Product {AggregateId}", 
                nameof(ProductDeletedEvent), domainEvent.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle {EventType} for Product {AggregateId}", 
                nameof(ProductDeletedEvent), domainEvent.ProductId);
            throw;
        }
    }
}