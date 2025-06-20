using ReadModelService.Application.Product.DTOs;
using ReadModelService.Application.Common;
using MassTransit;

namespace ReadModelService.Application.Product.Queries.GetProducts;

public class GetProductsQueryHandler : IConsumer<GetProductsQuery>
{
    private readonly IRepository<ReadModelService.Domain.Entities.Product> _repository;

    public GetProductsQueryHandler(IRepository<ReadModelService.Domain.Entities.Product> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetProductsQuery> context)
    {
        var query = context.Message;
        var entities = await _repository.GetAllAsync(context.CancellationToken);
        var result = entities.Select(MapToDto).ToList();
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