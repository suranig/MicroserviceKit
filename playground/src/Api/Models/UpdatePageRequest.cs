using System.ComponentModel.DataAnnotations;

namespace ReadModelService.Api.Models;

/// <summary>
/// Request model for updating Page
/// </summary>
public class UpdatePageRequest
{
public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}