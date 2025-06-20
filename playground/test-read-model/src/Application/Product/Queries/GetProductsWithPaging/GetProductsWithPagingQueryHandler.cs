using ReadModelService.Application.Product.DTOs;
using ReadModelService.Application.Common;
using MassTransit;

namespace ReadModelService.Application.Product.Queries.GetProductsWithPaging;

public class GetProductsWithPagingQueryHandler : IConsumer<GetProductsWithPagingQuery>
{
    private readonly IRepository<ReadModelService.Domain.Entities.Product> _repository;

    public GetProductsWithPagingQueryHandler(IRepository<ReadModelService.Domain.Entities.Product> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetProductsWithPagingQuery> context)
    {
        var query = context.Message;
        var pagedResult = await _repository.GetPagedAsync(query.Page, query.PageSize, context.CancellationToken);
        var result = new PagedResult<ProductDto>
        {
            Items = pagedResult.Items.Select(MapToDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
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