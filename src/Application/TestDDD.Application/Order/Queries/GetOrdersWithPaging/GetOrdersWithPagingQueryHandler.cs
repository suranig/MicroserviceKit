using ECommerce.OrderService.Application.Order.DTOs;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Order.Queries.GetOrdersWithPaging;

public class GetOrdersWithPagingQueryHandler
{
    private readonly IRepository<ECommerce.OrderService.Domain.Entities.Order> _repository;

    public GetOrdersWithPagingQueryHandler(IRepository<ECommerce.OrderService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersWithPagingQuery query, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);
        return new PagedResult<OrderDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}