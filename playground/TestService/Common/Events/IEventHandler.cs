using AggregateKit;

namespace TestService.Application.Common.Events;

public interface IEventHandler<in T> where T : DomainEventBase
{
    Task Handle(T domainEvent, CancellationToken cancellationToken = default);
}