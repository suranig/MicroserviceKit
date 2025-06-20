using Microsoft.EntityFrameworkCore;
using TestService.Domain.Test;
using TestService.Infrastructure.Persistence;

namespace TestService.Infrastructure.Repositories;

public class TestRepository : ITestRepository
{
    private readonly ApplicationDbContext _context;

    public TestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Test?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tests
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Test>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tests
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Test>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Tests
            .OrderBy(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tests.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Test entity, CancellationToken cancellationToken = default)
    {
        await _context.Tests.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Test entity, CancellationToken cancellationToken = default)
    {
        _context.Tests.Update(entity);
    }

    public async Task DeleteAsync(Test entity, CancellationToken cancellationToken = default)
    {
        _context.Tests.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tests
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}