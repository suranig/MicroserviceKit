using AggregateKit;

namespace SimpleService.Application.Common.Events;

public interface IEventHandler<in T> where T : DomainEventBase
{
    Task Handle(T domainEvent, CancellationToken cancellationToken = default);
}