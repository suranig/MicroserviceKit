using System.ComponentModel.DataAnnotations;

namespace MyApp.UserService.Api.Models;

/// <summary>
/// Request model for creating User
/// </summary>
public class CreateUserRequest
{
public string Email { get; set; }
    public string Name { get; set; }
}