using TestService.Domain.Test;

namespace TestService.Infrastructure.Repositories;

public interface ITestRepository
{
    Task<Test?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Test>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Test>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Test entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Test entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Test entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}