using TestService.Application.Order.DTOs;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Order.Queries.GetOrders;

public class GetOrdersQueryHandler : IConsumer<GetOrdersQuery>
{
    private readonly IRepository<TestService.Domain.Entities.Order> _repository;

    public GetOrdersQueryHandler(IRepository<TestService.Domain.Entities.Order> repository)
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

    private OrderDto MapToDto(TestService.Domain.Entities.Order entity)
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