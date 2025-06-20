using OrderService.Application.Order.DTOs;
using OrderService.Application.Common;
using MassTransit;

namespace OrderService.Application.Order.Queries.GetOrdersWithPaging;

public class GetOrdersWithPagingQueryHandler : IConsumer<GetOrdersWithPagingQuery>
{
    private readonly IRepository<OrderService.Domain.Entities.Order> _repository;

    public GetOrdersWithPagingQueryHandler(IRepository<OrderService.Domain.Entities.Order> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetOrdersWithPagingQuery> context)
    {
        var query = context.Message;
        var pagedResult = await _repository.GetPagedAsync(query.Page, query.PageSize, context.CancellationToken);
        var result = new PagedResult<OrderDto>
        {
            Items = pagedResult.Items.Select(MapToDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
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