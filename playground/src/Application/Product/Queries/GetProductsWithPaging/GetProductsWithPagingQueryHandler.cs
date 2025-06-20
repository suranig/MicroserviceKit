using TestService.Application.Product.DTOs;
using TestService.Application.Common;

namespace TestService.Application.Product.Queries.GetProductsWithPaging;

public class GetProductsWithPagingQueryHandler
{
    private readonly IRepository<TestService.Domain.Entities.Product> _repository;

    public GetProductsWithPagingQueryHandler(IRepository<TestService.Domain.Entities.Product> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsWithPagingQuery query, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);
        return new PagedResult<ProductDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}