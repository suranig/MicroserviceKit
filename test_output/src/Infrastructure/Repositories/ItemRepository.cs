using Microsoft.EntityFrameworkCore;
using Company.TestService.Domain.Item;
using Company.TestService.Infrastructure.Persistence;

namespace Company.TestService.Infrastructure.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly ApplicationDbContext _context;

    public ItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Item>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Item>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .OrderBy(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Items.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Item entity, CancellationToken cancellationToken = default)
    {
        await _context.Items.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Item entity, CancellationToken cancellationToken = default)
    {
        _context.Items.Update(entity);
    }

    public async Task DeleteAsync(Item entity, CancellationToken cancellationToken = default)
    {
        _context.Items.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}