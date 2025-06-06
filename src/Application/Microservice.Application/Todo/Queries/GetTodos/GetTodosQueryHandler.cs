using MediatR;
using Microservice.Domain.Entities;
using Microservice.Domain.Interfaces;

namespace Microservice.Application.Todo.Queries.GetTodos;

public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, IReadOnlyList<TodoItem>>
{
    private readonly ITodoRepository _repository;

    public GetTodosQueryHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<TodoItem>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(cancellationToken);
    }
}
