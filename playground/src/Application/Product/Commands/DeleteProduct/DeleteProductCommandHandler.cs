using TestService.Domain.Entities;
using TestService.Application.Common;

namespace TestService.Application.Product.Commands.DeleteProduct;

public class DeleteProductCommandHandler
{
    private readonly IRepository<Product> _repository;

    public DeleteProductCommandHandler(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("Product not found");
            
        await _repository.DeleteAsync(entity, cancellationToken);
    }
}