using FluentValidation;

namespace WorkflowService.Application.Workflow.Commands.DeleteWorkflow;

public class DeleteWorkflowCommandValidator : AbstractValidator<DeleteWorkflowCommand>
{
    public DeleteWorkflowCommandValidator()
    {
        // Add validation rules as needed
    }
}