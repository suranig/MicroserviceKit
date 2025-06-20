using ReadModelService.Application.Page.DTOs;
using ReadModelService.Application.Common;

namespace ReadModelService.Application.Page.Queries.GetPages;

public class GetPagesQueryHandler
{
    private readonly IRepository<ReadModelService.Domain.Entities.Page> _repository;

    public GetPagesQueryHandler(IRepository<ReadModelService.Domain.Entities.Page> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PageDto>> Handle(GetPagesQuery query, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }
}