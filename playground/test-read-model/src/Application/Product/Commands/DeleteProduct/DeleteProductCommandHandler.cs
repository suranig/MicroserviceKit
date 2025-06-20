using ReadModelService.Domain.Entities;
using ReadModelService.Application.Common;
using MassTransit;

namespace ReadModelService.Application.Product.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IConsumer<DeleteProductCommand>
{
    private readonly IRepository<ReadModelService.Domain.Entities.Product> _repository;

    public DeleteProductCommandHandler(IRepository<ReadModelService.Domain.Entities.Product> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<DeleteProductCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("Product not found");
            
        await _repository.DeleteAsync(entity, context.CancellationToken);
        
        
    }
}