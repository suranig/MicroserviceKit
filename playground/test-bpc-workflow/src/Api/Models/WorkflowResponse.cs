namespace WorkflowService.Api.Models;

/// <summary>
/// Response model for Workflow
/// </summary>
public class WorkflowResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}