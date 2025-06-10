using Company.TestService.Domain.Entities;
using Company.TestService.Application.Common;

namespace Company.TestService.Application.Item.Commands.CreateItem;

public class CreateItemCommandHandler
{
    private readonly IRepository<Item> _repository;

    public CreateItemCommandHandler(IRepository<Item> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateItemCommand command, CancellationToken cancellationToken)
    {
        var entity = new Item(command.Title, command.IsCompleted);
        await _repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}