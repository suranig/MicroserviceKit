using ECommerce.OrderService.Application.Order.DTOs;
using ECommerce.OrderService.Application.Common;

namespace ECommerce.OrderService.Application.Order.Queries.GetOrderById;

public class GetOrderByIdQueryHandler
{
    private readonly IRepository<ECommerce.OrderService.Domain.Entities.Order> _repository;

    public GetOrderByIdQueryHandler(IRepository<ECommerce.OrderService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(query.Id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }
}