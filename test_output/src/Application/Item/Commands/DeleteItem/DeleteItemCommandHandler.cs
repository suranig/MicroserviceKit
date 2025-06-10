using Company.TestService.Domain.Entities;
using Company.TestService.Application.Common;

namespace Company.TestService.Application.Item.Commands.DeleteItem;

public class DeleteItemCommandHandler
{
    private readonly IRepository<Item> _repository;

    public DeleteItemCommandHandler(IRepository<Item> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(DeleteItemCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("Item not found");
            
        await _repository.DeleteAsync(entity, cancellationToken);
    }
}