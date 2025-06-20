using WorkflowService.Domain.Entities;
using WorkflowService.Application.Common;
using MassTransit;

namespace WorkflowService.Application.Workflow.Commands.UpdateWorkflow;

public class UpdateWorkflowCommandHandler : IConsumer<UpdateWorkflowCommand>
{
    private readonly IRepository<WorkflowService.Domain.Entities.Workflow> _repository;

    public UpdateWorkflowCommandHandler(IRepository<WorkflowService.Domain.Entities.Workflow> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<UpdateWorkflowCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.Id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("Workflow not found");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, context.CancellationToken);
        
        
    }
}