using Microservice.Domain.Entities;
using Microservice.Domain.Interfaces;

namespace Microservice.Infrastructure.Repositories;

public class InMemoryTodoRepository : ITodoRepository
{
    private readonly List<TodoItem> _items = new();

    public Task AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        _items.Add(item);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<TodoItem>)_items);
    }
}
