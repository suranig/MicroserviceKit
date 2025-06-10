using Company.TestService.Application.Item.DTOs;
using Company.TestService.Application.Common;

namespace Company.TestService.Application.Item.Queries.GetItemsWithPaging;

public class GetItemsWithPagingQueryHandler
{
    private readonly IRepository<Company.TestService.Domain.Entities.Item> _repository;

    public GetItemsWithPagingQueryHandler(IRepository<Company.TestService.Domain.Entities.Item> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ItemDto>> Handle(GetItemsWithPagingQuery query, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);
        return new PagedResult<ItemDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}