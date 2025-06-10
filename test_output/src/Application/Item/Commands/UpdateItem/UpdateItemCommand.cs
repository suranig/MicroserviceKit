namespace Company.TestService.Application.Item.Commands.UpdateItem;

public record UpdateItemCommand(Guid id, string title, bool isCompleted);