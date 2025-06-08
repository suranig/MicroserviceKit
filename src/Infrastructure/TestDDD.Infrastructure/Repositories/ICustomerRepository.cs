using ECommerce.OrderService.Domain.Customer;

namespace ECommerce.OrderService.Infrastructure.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Customer entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Customer entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}