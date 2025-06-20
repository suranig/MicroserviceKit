using ReadModelService.Domain.Entities;
using ReadModelService.Application.Common;

namespace ReadModelService.Application.Page.Commands.DeletePage;

public class DeletePageCommandHandler
{
    private readonly IRepository<Page> _repository;

    public DeletePageCommandHandler(IRepository<Page> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(DeletePageCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("Page not found");
            
        await _repository.DeleteAsync(entity, cancellationToken);
    }
}