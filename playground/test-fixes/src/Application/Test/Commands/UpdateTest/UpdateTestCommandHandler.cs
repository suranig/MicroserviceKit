using TestService.Domain.Entities;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Test.Commands.UpdateTest;

public class UpdateTestCommandHandler : IConsumer<UpdateTestCommand>
{
    private readonly IRepository<TestService.Domain.Entities.Test> _repository;

    public UpdateTestCommandHandler(IRepository<TestService.Domain.Entities.Test> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<UpdateTestCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.Id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("Test not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, context.CancellationToken);
        
        
    }
}