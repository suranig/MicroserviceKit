using TestService.Domain.Entities;
using TestService.Application.Common;
using MassTransit;

namespace TestService.Application.Test.Commands.CreateTest;

public class CreateTestCommandHandler : IConsumer<CreateTestCommand>
{
    private readonly IRepository<TestService.Domain.Entities.Test> _repository;

    public CreateTestCommandHandler(IRepository<TestService.Domain.Entities.Test> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CreateTestCommand> context)
    {
        var command = context.Message;
        // Create entity - adjust constructor parameters based on your domain entity
        var entity = new TestService.Domain.Entities.Test(
            Guid.NewGuid()
            , command.Name, command.Description
        );
        
        // Set audit fields if they exist
        // entity.CreatedAt = DateTime.UtcNow;
        
        await _repository.AddAsync(entity, context.CancellationToken);
        
        await context.RespondAsync(entity.Id);
    }
}