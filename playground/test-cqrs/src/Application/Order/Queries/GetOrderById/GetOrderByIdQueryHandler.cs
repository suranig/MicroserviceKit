using OrderService.Application.Order.DTOs;
using OrderService.Application.Common;
using MassTransit;

namespace OrderService.Application.Order.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IConsumer<GetOrderByIdQuery>
{
    private readonly IRepository<OrderService.Domain.Entities.Order> _repository;

    public GetOrderByIdQueryHandler(IRepository<OrderService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetOrderByIdQuery> context)
    {
        var query = context.Message;
        var entity = await _repository.GetByIdAsync(query.Id, context.CancellationToken);
        var result = entity == null ? null : MapToDto(entity);
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