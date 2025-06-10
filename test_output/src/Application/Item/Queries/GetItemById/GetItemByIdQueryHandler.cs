using Company.TestService.Application.Item.DTOs;
using Company.TestService.Application.Common;

namespace Company.TestService.Application.Item.Queries.GetItemById;

public class GetItemByIdQueryHandler
{
    private readonly IRepository<Company.TestService.Domain.Entities.Item> _repository;

    public GetItemByIdQueryHandler(IRepository<Company.TestService.Domain.Entities.Item> repository)
    {
        _repository = repository;
    }

    public async Task<ItemDto?> Handle(GetItemByIdQuery query, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(query.Id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }
}