using Microsoft.Extensions.Logging;
using SimpleService.Application.Common.Events;
using SimpleService.Application.User.ReadModels;
using SimpleService.Domain.Events;

namespace SimpleService.Application.User.EventHandlers;

/// <summary>
/// Updates read models when User domain events occur
/// </summary>
public class UserReadModelUpdater : 
    IEventHandler<UserCreatedEvent>,
    IEventHandler<UserUpdatedEvent>,
    IEventHandler<UserDeletedEvent>
{
    private readonly ILogger<UserReadModelUpdater> _logger;
    // TODO: Add read model repository dependency

    public UserReadModelUpdater(ILogger<UserReadModelUpdater> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating read model for created User {AggregateId}", 
            domainEvent.UserId);

        // TODO: Create read model entry
        var readModel = new UserReadModel
        {
            Id = domainEvent.UserId,
            Id = domainEvent.Id,
            Name = domainEvent.Name,
            Description = domainEvent.Description
        };

        // TODO: Save to read model store (MongoDB, etc.)
        
        await Task.CompletedTask;
    }

    public async Task Handle(UserUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating read model for updated User {AggregateId}", 
            domainEvent.UserId);

        // TODO: Update existing read model entry
        
        await Task.CompletedTask;
    }

    public async Task Handle(UserDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating read model for deleted User {AggregateId}", 
            domainEvent.UserId);

        // TODO: Remove read model entry
        
        await Task.CompletedTask;
    }
}