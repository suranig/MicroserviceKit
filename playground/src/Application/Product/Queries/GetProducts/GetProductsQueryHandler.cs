using TestService.Application.Product.DTOs;
using TestService.Application.Common;

namespace TestService.Application.Product.Queries.GetProducts;

public class GetProductsQueryHandler
{
    private readonly IRepository<TestService.Domain.Entities.Product> _repository;

    public GetProductsQueryHandler(IRepository<TestService.Domain.Entities.Product> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }
}