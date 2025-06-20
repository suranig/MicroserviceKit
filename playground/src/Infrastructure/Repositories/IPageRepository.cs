using ReadModelService.Domain.Page;

namespace ReadModelService.Infrastructure.Repositories;

public interface IPageRepository
{
    Task<Page?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Page>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Page>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Page entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Page entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Page entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}