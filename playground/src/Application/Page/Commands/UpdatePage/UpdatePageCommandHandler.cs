using ReadModelService.Domain.Entities;
using ReadModelService.Application.Common;

namespace ReadModelService.Application.Page.Commands.UpdatePage;

public class UpdatePageCommandHandler
{
    private readonly IRepository<Page> _repository;

    public UpdatePageCommandHandler(IRepository<Page> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(UpdatePageCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("Page not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, cancellationToken);
    }
}