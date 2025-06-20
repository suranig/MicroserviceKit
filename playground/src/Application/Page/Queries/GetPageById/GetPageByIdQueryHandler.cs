using ReadModelService.Application.Page.DTOs;
using ReadModelService.Application.Common;

namespace ReadModelService.Application.Page.Queries.GetPageById;

public class GetPageByIdQueryHandler
{
    private readonly IRepository<ReadModelService.Domain.Entities.Page> _repository;

    public GetPageByIdQueryHandler(IRepository<ReadModelService.Domain.Entities.Page> repository)
    {
        _repository = repository;
    }

    public async Task<PageDto?> Handle(GetPageByIdQuery query, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(query.Id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }
}