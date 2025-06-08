using ECommerce.OrderService.Application.Order.DTOs;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Order.Queries.GetOrders;

public class GetOrdersQueryHandler
{
    private readonly IRepository<ECommerce.OrderService.Domain.Entities.Order> _repository;

    public GetOrdersQueryHandler(IRepository<ECommerce.OrderService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }
}