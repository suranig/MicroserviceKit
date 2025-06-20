namespace WorkflowService.Application.Workflow.Commands.UpdateWorkflow;

public record UpdateWorkflowCommand(Guid Id, string Name, string Description);