using Microservice.Domain.Entities;
using Microservice.Domain.Interfaces;

namespace Microservice.Application.Todo.Commands.CreateTodo;

public class CreateTodoCommandHandler
{
    private readonly ITodoRepository _repository;

    public CreateTodoCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateTodoCommand command, CancellationToken cancellationToken)
    {
        var item = new TodoItem(command.Title);
        await _repository.AddAsync(item, cancellationToken);
        return item.Id;
    }
}
