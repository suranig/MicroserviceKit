using TestService.Domain.Entities;
using TestService.Application.Common;

namespace TestService.Application.Product.Commands.UpdateProduct;

public class UpdateProductCommandHandler
{
    private readonly IRepository<Product> _repository;

    public UpdateProductCommandHandler(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("Product not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, cancellationToken);
    }
}