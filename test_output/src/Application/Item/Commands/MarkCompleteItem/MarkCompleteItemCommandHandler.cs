using Company.TestService.Domain.Entities;
using Company.TestService.Application.Common;

namespace Company.TestService.Application.Item.Commands.MarkCompleteItem;

public class MarkCompleteItemCommandHandler
{
    private readonly IRepository<Item> _repository;

    public MarkCompleteItemCommandHandler(IRepository<Item> repository)
    {
        _repository = repository;
    }

    public async Task<void> Handle(MarkCompleteItemCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implement MarkComplete logic
    }
}