using System.ComponentModel.DataAnnotations;

namespace TestService.Api.Models;

/// <summary>
/// Request model for creating Test
/// </summary>
public class CreateTestRequest
{
public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}