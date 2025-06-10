using Company.TestService.Domain.Entities;
using Company.TestService.Application.Common;

namespace Company.TestService.Application.Item.Commands.UpdateItem;

public class UpdateItemCommandHandler
{
    private readonly IRepository<Item> _repository;

    public UpdateItemCommandHandler(IRepository<Item> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(UpdateItemCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("Item not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, cancellationToken);
    }
}