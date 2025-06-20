using TestService.Application.Order.DTOs;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Order.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IConsumer<GetOrderByIdQuery>
{
    private readonly IRepository<TestService.Domain.Entities.Order> _repository;

    public GetOrderByIdQueryHandler(IRepository<TestService.Domain.Entities.Order> repository)
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