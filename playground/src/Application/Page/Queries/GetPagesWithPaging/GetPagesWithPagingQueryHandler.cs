using ReadModelService.Application.Page.DTOs;
using ReadModelService.Application.Common;

namespace ReadModelService.Application.Page.Queries.GetPagesWithPaging;

public class GetPagesWithPagingQueryHandler
{
    private readonly IRepository<ReadModelService.Domain.Entities.Page> _repository;

    public GetPagesWithPagingQueryHandler(IRepository<ReadModelService.Domain.Entities.Page> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<PageDto>> Handle(GetPagesWithPagingQuery query, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);
        return new PagedResult<PageDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}