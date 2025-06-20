using SimpleService.Domain.Entities;
using SimpleService.Application.Common;
using MassTransit;

namespace SimpleService.Application.User.Commands.DeleteUser;

public class DeleteUserCommandHandler : IConsumer<DeleteUserCommand>
{
    private readonly IRepository<SimpleService.Domain.Entities.User> _repository;

    public DeleteUserCommandHandler(IRepository<SimpleService.Domain.Entities.User> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<DeleteUserCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("User not found");
            
        await _repository.DeleteAsync(entity, context.CancellationToken);
        
        
    }
}