using System.ComponentModel.DataAnnotations;

namespace WorkflowService.Api.Models;

/// <summary>
/// Request model for updating Workflow
/// </summary>
public class UpdateWorkflowRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}