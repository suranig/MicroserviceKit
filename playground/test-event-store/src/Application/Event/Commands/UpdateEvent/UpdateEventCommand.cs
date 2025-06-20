namespace EventStoreService.Application.Event.Commands.UpdateEvent;

public record UpdateEventCommand(Guid Id, string Name, string Description);