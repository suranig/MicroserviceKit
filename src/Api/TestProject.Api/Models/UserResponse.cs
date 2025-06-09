namespace MyApp.UserService.Api.Models;

/// <summary>
/// Response model for User
/// </summary>
public class UserResponse
{
    public Guid Id { get; set; }
public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}