using TestService.Domain.Entities;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Test.Commands.DeleteTest;

public class DeleteTestCommandHandler : IConsumer<DeleteTestCommand>
{
    private readonly IRepository<TestService.Domain.Entities.Test> _repository;

    public DeleteTestCommandHandler(IRepository<TestService.Domain.Entities.Test> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<DeleteTestCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.Id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("Test not found");
            
        await _repository.DeleteAsync(entity, context.CancellationToken);
        
        
    }
}