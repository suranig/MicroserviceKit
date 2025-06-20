using System.ComponentModel.DataAnnotations;

namespace WorkflowService.Api.Models;

/// <summary>
/// Request model for creating Workflow
/// </summary>
public class CreateWorkflowRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}