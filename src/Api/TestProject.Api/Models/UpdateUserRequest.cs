using System.ComponentModel.DataAnnotations;

namespace MyApp.UserService.Api.Models;

/// <summary>
/// Request model for updating User
/// </summary>
public class UpdateUserRequest
{
public string Email { get; set; }
    public string Name { get; set; }
}