using OrderService.Application.Order.DTOs;
using OrderService.Application.Common;
using MassTransit;

namespace OrderService.Application.Order.Queries.GetOrders;

public class GetOrdersQueryHandler : IConsumer<GetOrdersQuery>
{
    private readonly IRepository<OrderService.Domain.Entities.Order> _repository;

    public GetOrdersQueryHandler(IRepository<OrderService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetOrdersQuery> context)
    {
        var query = context.Message;
        var entities = await _repository.GetAllAsync(context.CancellationToken);
        var result = entities.Select(MapToDto).ToList();
        await context.RespondAsync(result);
    }

    private OrderDto MapToDto(OrderService.Domain.Entities.Order entity)
    {
        return new OrderDto
        {
            Id = entity.Id,
            // TODO: Map other properties
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}