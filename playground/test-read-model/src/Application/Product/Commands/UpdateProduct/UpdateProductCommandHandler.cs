using ReadModelService.Domain.Entities;
using ReadModelService.Application.Common;
using MassTransit;

namespace ReadModelService.Application.Product.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IConsumer<UpdateProductCommand>
{
    private readonly IRepository<ReadModelService.Domain.Entities.Product> _repository;

    public UpdateProductCommandHandler(IRepository<ReadModelService.Domain.Entities.Product> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<UpdateProductCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.Id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("Product not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, context.CancellationToken);
        
        
    }
}