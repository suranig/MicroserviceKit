using Microsoft.EntityFrameworkCore;
using ReadModelService.Domain.Page;
using ReadModelService.Infrastructure.Persistence;

namespace ReadModelService.Infrastructure.Repositories;

public class PageRepository : IPageRepository
{
    private readonly ApplicationDbContext _context;

    public PageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Page?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pages
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Page>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pages
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Page>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Pages
            .OrderBy(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pages.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Page entity, CancellationToken cancellationToken = default)
    {
        await _context.Pages.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Page entity, CancellationToken cancellationToken = default)
    {
        _context.Pages.Update(entity);
    }

    public async Task DeleteAsync(Page entity, CancellationToken cancellationToken = default)
    {
        _context.Pages.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pages
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}