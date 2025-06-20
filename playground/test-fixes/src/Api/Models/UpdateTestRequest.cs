using System.ComponentModel.DataAnnotations;

namespace TestService.Api.Models;

/// <summary>
/// Request model for updating Test
/// </summary>
public class UpdateTestRequest
{
public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}