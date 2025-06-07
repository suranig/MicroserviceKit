using MediatR;

namespace Microservice.Application.Todo.Commands.CreateTodo;

public record CreateTodoCommand(string Title) : IRequest<Guid>;
