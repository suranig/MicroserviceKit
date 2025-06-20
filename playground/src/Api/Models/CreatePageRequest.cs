using System.ComponentModel.DataAnnotations;

namespace ReadModelService.Api.Models;

/// <summary>
/// Request model for creating Page
/// </summary>
public class CreatePageRequest
{
public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}