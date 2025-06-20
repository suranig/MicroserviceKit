using Microsoft.EntityFrameworkCore;
using TestService.Infrastructure.Persistence;

namespace TestService.Infrastructure.Messaging.Publishers;

public interface IOutboxRepository
{
    Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);
    Task<List<OutboxEvent>> GetUnprocessedEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid eventId, string errorMessage, CancellationToken cancellationToken = default);
    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
}

public class OutboxRepository : IOutboxRepository
{
    private readonly ApplicationDbContext _context;

    public OutboxRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default)
    {
        _context.OutboxEvents.Add(outboxEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<OutboxEvent>> GetUnprocessedEventsAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxEvents
            .Where(e => !e.IsProcessed && e.RetryCount < 5)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var outboxEvent = await _context.OutboxEvents.FindAsync(new object[] { eventId }, cancellationToken);
        if (outboxEvent != null)
        {
            outboxEvent.IsProcessed = true;
            outboxEvent.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsFailedAsync(Guid eventId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var outboxEvent = await _context.OutboxEvents.FindAsync(new object[] { eventId }, cancellationToken);
        if (outboxEvent != null)
        {
            outboxEvent.RetryCount++;
            outboxEvent.ErrorMessage = errorMessage;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OutboxEvents
            .CountAsync(e => !e.IsProcessed, cancellationToken);
    }
}