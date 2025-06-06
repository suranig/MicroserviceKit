using Microservice.Domain.Entities;

namespace Microservice.Domain.Interfaces;

public interface ITodoRepository
{
    Task AddAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);
}
