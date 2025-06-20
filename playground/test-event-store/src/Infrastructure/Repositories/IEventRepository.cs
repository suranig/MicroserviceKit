using EventStoreService.Domain.Entities;

namespace EventStoreService.Infrastructure.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Event entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Event entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Event entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}