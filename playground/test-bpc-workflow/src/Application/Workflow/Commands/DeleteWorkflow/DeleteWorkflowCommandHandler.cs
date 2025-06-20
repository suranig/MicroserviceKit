using WorkflowService.Domain.Entities;
using WorkflowService.Application.Common;
using MassTransit;

namespace WorkflowService.Application.Workflow.Commands.DeleteWorkflow;

public class DeleteWorkflowCommandHandler : IConsumer<DeleteWorkflowCommand>
{
    private readonly IRepository<WorkflowService.Domain.Entities.Workflow> _repository;

    public DeleteWorkflowCommandHandler(IRepository<WorkflowService.Domain.Entities.Workflow> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<DeleteWorkflowCommand> context)
    {
        var command = context.Message;
        var entity = await _repository.GetByIdAsync(command.id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException("Workflow not found");
            
        await _repository.DeleteAsync(entity, context.CancellationToken);
        
        
    }
}