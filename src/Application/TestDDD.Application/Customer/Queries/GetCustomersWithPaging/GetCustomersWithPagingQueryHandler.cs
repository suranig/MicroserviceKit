using ECommerce.OrderService.Application.Customer.DTOs;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Customer.Queries.GetCustomersWithPaging;

public class GetCustomersWithPagingQueryHandler
{
    private readonly IRepository<ECommerce.OrderService.Domain.Entities.Customer> _repository;

    public GetCustomersWithPagingQueryHandler(IRepository<ECommerce.OrderService.Domain.Entities.Customer> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersWithPagingQuery query, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);
        return new PagedResult<CustomerDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}