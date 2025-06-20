using ReadModelService.Domain.Entities;
using ReadModelService.Application.Common;
using MassTransit;

namespace ReadModelService.Application.Product.Commands.CreateProduct;

public class CreateProductCommandHandler : IConsumer<CreateProductCommand>
{
    private readonly IRepository<ReadModelService.Domain.Entities.Product> _repository;

    public CreateProductCommandHandler(IRepository<ReadModelService.Domain.Entities.Product> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CreateProductCommand> context)
    {
        var command = context.Message;
        var entity = new ReadModelService.Domain.Entities.Product(Guid.NewGuid(), command.Name, command.Description);
        await _repository.AddAsync(entity, context.CancellationToken);
        
        await context.RespondAsync(entity.Id);
    }
}