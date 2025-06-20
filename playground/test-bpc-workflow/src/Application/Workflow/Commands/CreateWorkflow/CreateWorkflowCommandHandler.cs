using WorkflowService.Domain.Entities;
using WorkflowService.Application.Common;
using MassTransit;

namespace WorkflowService.Application.Workflow.Commands.CreateWorkflow;

public class CreateWorkflowCommandHandler : IConsumer<CreateWorkflowCommand>
{
    private readonly IRepository<WorkflowService.Domain.Entities.Workflow> _repository;

    public CreateWorkflowCommandHandler(IRepository<WorkflowService.Domain.Entities.Workflow> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CreateWorkflowCommand> context)
    {
        var command = context.Message;
        var entity = new WorkflowService.Domain.Entities.Workflow(Guid.NewGuid(), command.Name, command.Description);
        await _repository.AddAsync(entity, context.CancellationToken);
        
        await context.RespondAsync(entity.Id);
    }
}