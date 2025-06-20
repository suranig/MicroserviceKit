using WorkflowService.Domain.Entities;

namespace WorkflowService.Infrastructure.Repositories;

public interface IWorkflowRepository
{
    Task<Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workflow>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workflow>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Workflow entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Workflow entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Workflow entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}