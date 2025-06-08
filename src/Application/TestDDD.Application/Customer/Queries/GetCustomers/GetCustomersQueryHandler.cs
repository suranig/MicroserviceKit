using ECommerce.OrderService.Application.Customer.DTOs;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Customer.Queries.GetCustomers;

public class GetCustomersQueryHandler
{
    private readonly IRepository<ECommerce.OrderService.Domain.Entities.Customer> _repository;

    public GetCustomersQueryHandler(IRepository<ECommerce.OrderService.Domain.Entities.Customer> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CustomerDto>> Handle(GetCustomersQuery query, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }
}