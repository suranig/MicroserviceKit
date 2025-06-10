using Company.TestService.Application.Item.DTOs;
using Company.TestService.Application.Common;

namespace Company.TestService.Application.Item.Queries.GetItems;

public class GetItemsQueryHandler
{
    private readonly IRepository<Company.TestService.Domain.Entities.Item> _repository;

    public GetItemsQueryHandler(IRepository<Company.TestService.Domain.Entities.Item> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ItemDto>> Handle(GetItemsQuery query, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }
}