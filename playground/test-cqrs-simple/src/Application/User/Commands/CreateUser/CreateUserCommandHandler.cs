using SimpleService.Domain.Entities;
using SimpleService.Application.Common;
using MassTransit;

namespace SimpleService.Application.User.Commands.CreateUser;

public class CreateUserCommandHandler : IConsumer<CreateUserCommand>
{
    private readonly IRepository<SimpleService.Domain.Entities.User> _repository;

    public CreateUserCommandHandler(IRepository<SimpleService.Domain.Entities.User> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CreateUserCommand> context)
    {
        var command = context.Message;
        var entity = new SimpleService.Domain.Entities.User(Guid.NewGuid(), command.Name, command.Description);
        await _repository.AddAsync(entity, context.CancellationToken);
        
        await context.RespondAsync(entity.Id);
    }
}