using TestService.Application.Product.DTOs;
using TestService.Application.Common;

namespace TestService.Application.Product.Queries.GetProductById;

public class GetProductByIdQueryHandler
{
    private readonly IRepository<TestService.Domain.Entities.Product> _repository;

    public GetProductByIdQueryHandler(IRepository<TestService.Domain.Entities.Product> repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(query.Id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }
}