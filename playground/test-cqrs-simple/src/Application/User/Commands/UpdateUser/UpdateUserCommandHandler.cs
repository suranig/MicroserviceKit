using SimpleService.Domain.Entities;
using SimpleService.Application.Common;
using MassTransit;

namespace SimpleService.Application.User.Commands.UpdateUser;

public class UpdateUserCommandHandler : IConsumer<UpdateUserCommand>
{
    private readonly IRepository<SimpleService.Domain.Entities.User> _repository;

    public UpdateUserCommandHandler(IRepository<SimpleService.Domain.Entities.User> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<UpdateUserCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.Id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("User not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, context.CancellationToken);
        
        
    }
}