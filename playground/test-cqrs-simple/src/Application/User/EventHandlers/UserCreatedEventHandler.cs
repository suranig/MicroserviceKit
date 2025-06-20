using Microsoft.Extensions.Logging;
using SimpleService.Application.Common.Events;
using SimpleService.Domain.Events;

namespace SimpleService.Application.User.EventHandlers;

public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling {EventType} for User {AggregateId}", 
            nameof(UserCreatedEvent), domainEvent.UserId);

        try
        {
            // TODO: Implement business logic for UserCreatedEvent
            // Examples:
            // - Update read models
            // - Send notifications
            // - Trigger workflows
            // - Update caches
            
            await Task.CompletedTask;
            
            _logger.LogInformation("Successfully handled {EventType} for User {AggregateId}", 
                nameof(UserCreatedEvent), domainEvent.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle {EventType} for User {AggregateId}", 
                nameof(UserCreatedEvent), domainEvent.UserId);
            throw;
        }
    }
}