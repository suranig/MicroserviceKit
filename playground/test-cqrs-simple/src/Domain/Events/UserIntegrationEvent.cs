using AggregateKit;

namespace SimpleService.Domain.Events;

/// <summary>
/// Integration event for User - published to external services
/// </summary>
public record UserIntegrationEvent(
    Guid UserId,
    string EventType,
    Guid Id, string Name, string Description,
    DateTime OccurredAt
) : DomainEventBase;