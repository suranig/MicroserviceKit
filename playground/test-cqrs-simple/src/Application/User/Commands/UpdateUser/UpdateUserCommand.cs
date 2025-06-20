namespace SimpleService.Application.User.Commands.UpdateUser;

public record UpdateUserCommand(Guid Id, string Name, string Description);