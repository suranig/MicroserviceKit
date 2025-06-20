using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AggregateKit;

namespace SimpleService.Application.Common.Events;

public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventDispatcher> _logger;

    public EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync<T>(T domainEvent, CancellationToken cancellationToken = default) 
        where T : DomainEventBase
    {
        _logger.LogDebug("Dispatching domain event {EventType} with ID {EventId}", 
            typeof(T).Name, domainEvent.EventId);

        var handlers = _serviceProvider.GetServices<IEventHandler<T>>();
        
        var tasks = handlers.Select(handler => 
            ExecuteHandlerSafely(handler, domainEvent, cancellationToken));
            
        await Task.WhenAll(tasks);
        
        _logger.LogDebug("Completed dispatching domain event {EventType} to {HandlerCount} handlers", 
            typeof(T).Name, handlers.Count());
    }

    public async Task DispatchAsync(IEnumerable<DomainEventBase> domainEvents, CancellationToken cancellationToken = default)
    {
        var tasks = domainEvents.Select(domainEvent => 
            DispatchSingleEvent(domainEvent, cancellationToken));
            
        await Task.WhenAll(tasks);
    }

    private async Task DispatchSingleEvent(DomainEventBase domainEvent, CancellationToken cancellationToken)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType);

        var tasks = handlers.Select(handler => 
            ExecuteHandlerSafely(handler, domainEvent, cancellationToken));
            
        await Task.WhenAll(tasks);
    }

    private async Task ExecuteHandlerSafely(object handler, DomainEventBase domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            var handleMethod = handler.GetType().GetMethod("Handle");
            if (handleMethod != null)
            {
                var task = (Task)handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
                await task;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing event handler {HandlerType} for event {EventType}", 
                handler.GetType().Name, domainEvent.GetType().Name);
            // Don't rethrow - we want other handlers to continue executing
        }
    }
}