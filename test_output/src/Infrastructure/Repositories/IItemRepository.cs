using Company.TestService.Domain.Item;

namespace Company.TestService.Infrastructure.Repositories;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Item>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Item entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Item entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Item entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}