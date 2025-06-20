using AggregateKit;

namespace SimpleService.Application.Common.Events;

public interface IEventDispatcher
{
    Task DispatchAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : DomainEventBase;
    Task DispatchAsync(IEnumerable<DomainEventBase> domainEvents, CancellationToken cancellationToken = default);
}