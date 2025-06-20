using TestService.Domain.Entities;
using TestService.Application.Common;

namespace TestService.Application.Product.Commands.CreateProduct;

public class CreateProductCommandHandler
{
    private readonly IRepository<Product> _repository;

    public CreateProductCommandHandler(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var entity = new Product(command.Id, command.CreatedAt, command.UpdatedAt);
        await _repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}