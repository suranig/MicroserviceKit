using ReadModelService.Application.Product.DTOs;
using ReadModelService.Application.Common;
using MassTransit;

namespace ReadModelService.Application.Product.Queries.GetProductById;

public class GetProductByIdQueryHandler : IConsumer<GetProductByIdQuery>
{
    private readonly IRepository<ReadModelService.Domain.Entities.Product> _repository;

    public GetProductByIdQueryHandler(IRepository<ReadModelService.Domain.Entities.Product> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetProductByIdQuery> context)
    {
        var query = context.Message;
        var entity = await _repository.GetByIdAsync(query.Id, context.CancellationToken);
        var result = entity == null ? null : MapToDto(entity);
        await context.RespondAsync(result);
    }

    private ProductDto MapToDto(ReadModelService.Domain.Entities.Product entity)
    {
        return new ProductDto
        {
            Id = entity.Id,
            // TODO: Map other properties
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}