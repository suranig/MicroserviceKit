namespace Company.TestService.Application.Item.Commands.MarkCompleteItem;

public record MarkCompleteItemCommand(string title, bool isCompleted);