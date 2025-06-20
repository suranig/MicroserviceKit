using ReadModelService.Domain.Entities;

namespace ReadModelService.Infrastructure.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Product entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Product entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}