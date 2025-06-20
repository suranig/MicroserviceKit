using AggregateKit;

namespace TestService.Domain.Events;

/// <summary>
/// Integration event for Test - published to external services
/// </summary>
public record TestIntegrationEvent(
    Guid TestId,
    string EventType,
    Guid Id, string Name, string Description, DateTime CreatedAt, DateTime UpdatedAt,
    DateTime OccurredAt
) : DomainEventBase;