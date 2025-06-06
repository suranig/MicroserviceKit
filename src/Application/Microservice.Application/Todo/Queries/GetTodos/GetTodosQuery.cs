using MediatR;
using Microservice.Domain.Entities;

namespace Microservice.Application.Todo.Queries.GetTodos;

public record GetTodosQuery() : IRequest<IReadOnlyList<TodoItem>>;
