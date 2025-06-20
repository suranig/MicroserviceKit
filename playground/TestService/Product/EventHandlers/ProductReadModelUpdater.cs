using Microsoft.Extensions.Logging;
using TestService.Application.Common.Events;
using TestService.Application.Product.ReadModels;
using TestService.Domain.Events;

namespace TestService.Application.Product.EventHandlers;

/// <summary>
/// Updates read models when Product domain events occur
/// </summary>
public class ProductReadModelUpdater : 
    IEventHandler<ProductCreatedEvent>,
    IEventHandler<ProductUpdatedEvent>,
    IEventHandler<ProductDeletedEvent>
{
    private readonly ILogger<ProductReadModelUpdater> _logger;
    // TODO: Add read model repository dependency

    public ProductReadModelUpdater(ILogger<ProductReadModelUpdater> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating read model for created Product {AggregateId}", 
            domainEvent.ProductId);

        // TODO: Create read model entry
        var readModel = new ProductReadModel
        {
            Id = domainEvent.ProductId,
            Id = domainEvent.Id,
            CreatedAt = domainEvent.CreatedAt,
            UpdatedAt = domainEvent.UpdatedAt
        };

        // TODO: Save to read model store (MongoDB, etc.)
        
        await Task.CompletedTask;
    }

    public async Task Handle(ProductUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating read model for updated Product {AggregateId}", 
            domainEvent.ProductId);

        // TODO: Update existing read model entry
        
        await Task.CompletedTask;
    }

    public async Task Handle(ProductDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating read model for deleted Product {AggregateId}", 
            domainEvent.ProductId);

        // TODO: Remove read model entry
        
        await Task.CompletedTask;
    }
}