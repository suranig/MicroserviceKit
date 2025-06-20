using ReadModelService.Domain.Entities;
using ReadModelService.Application.Common;

namespace ReadModelService.Application.Page.Commands.CreatePage;

public class CreatePageCommandHandler
{
    private readonly IRepository<Page> _repository;

    public CreatePageCommandHandler(IRepository<Page> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreatePageCommand command, CancellationToken cancellationToken)
    {
        var entity = new Page(command.Id, command.CreatedAt, command.UpdatedAt);
        await _repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}