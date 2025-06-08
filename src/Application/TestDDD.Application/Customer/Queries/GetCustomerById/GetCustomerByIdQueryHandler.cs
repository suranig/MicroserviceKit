using ECommerce.OrderService.Application.Customer.DTOs;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Customer.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler
{
    private readonly IRepository<ECommerce.OrderService.Domain.Entities.Customer> _repository;

    public GetCustomerByIdQueryHandler(IRepository<ECommerce.OrderService.Domain.Entities.Customer> repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(query.Id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }
}