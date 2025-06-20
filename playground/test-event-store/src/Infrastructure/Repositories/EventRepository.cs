using Microsoft.EntityFrameworkCore;
using EventStoreService.Domain.Entities;
using EventStoreService.Infrastructure.Persistence;

namespace EventStoreService.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .OrderBy(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Event entity, CancellationToken cancellationToken = default)
    {
        await _context.Events.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Event entity, CancellationToken cancellationToken = default)
    {
        _context.Events.Update(entity);
    }

    public async Task DeleteAsync(Event entity, CancellationToken cancellationToken = default)
    {
        _context.Events.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}