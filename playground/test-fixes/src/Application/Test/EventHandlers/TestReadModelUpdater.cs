using Microsoft.Extensions.Logging;
using TestService.Application.Common.Events;
using TestService.Application.Test.ReadModels;
using TestService.Domain.Events;

namespace TestService.Application.Test.EventHandlers;

/// <summary>
/// Updates read models when Test domain events occur
/// </summary>
public class TestReadModelUpdater : 
    IEventHandler<TestCreatedEvent>,
    IEventHandler<TestUpdatedEvent>,
    IEventHandler<TestDeletedEvent>
{
    private readonly ILogger<TestReadModelUpdater> _logger;
    // TODO: Add read model repository dependency

    public TestReadModelUpdater(ILogger<TestReadModelUpdater> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TestCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating read model for created Test {AggregateId}", 
            domainEvent.TestId);

        // TODO: Create read model entry
        var readModel = new TestReadModel
        {
            Id = domainEvent.TestId,
            Id = domainEvent.Id,
            Name = domainEvent.Name,
            Description = domainEvent.Description,
            CreatedAt = domainEvent.CreatedAt,
            UpdatedAt = domainEvent.UpdatedAt
        };

        // TODO: Save to read model store (MongoDB, etc.)
        
        await Task.CompletedTask;
    }

    public async Task Handle(TestUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating read model for updated Test {AggregateId}", 
            domainEvent.TestId);

        // TODO: Update existing read model entry
        
        await Task.CompletedTask;
    }

    public async Task Handle(TestDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating read model for deleted Test {AggregateId}", 
            domainEvent.TestId);

        // TODO: Remove read model entry
        
        await Task.CompletedTask;
    }
}