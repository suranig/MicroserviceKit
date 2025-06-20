using Microsoft.EntityFrameworkCore;
using WorkflowService.Domain.Entities;
using WorkflowService.Infrastructure.Persistence;

namespace WorkflowService.Infrastructure.Repositories;

public class WorkflowRepository : IWorkflowRepository
{
    private readonly ApplicationDbContext _context;

    public WorkflowRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Workflow>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Workflow>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .OrderBy(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workflows.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Workflow entity, CancellationToken cancellationToken = default)
    {
        await _context.Workflows.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Workflow entity, CancellationToken cancellationToken = default)
    {
        _context.Workflows.Update(entity);
    }

    public async Task DeleteAsync(Workflow entity, CancellationToken cancellationToken = default)
    {
        _context.Workflows.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}