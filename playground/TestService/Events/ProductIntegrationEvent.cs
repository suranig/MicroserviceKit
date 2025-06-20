using AggregateKit;

namespace TestService.Domain.Events;

/// <summary>
/// Integration event for Product - published to external services
/// </summary>
public record ProductIntegrationEvent(
    Guid ProductId,
    string EventType,
    Guid Id, DateTime CreatedAt, DateTime UpdatedAt,
    DateTime OccurredAt
) : DomainEventBase;