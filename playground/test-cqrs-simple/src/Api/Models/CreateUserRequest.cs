using System.ComponentModel.DataAnnotations;

namespace SimpleService.Api.Models;

/// <summary>
/// Request model for creating User
/// </summary>
public class CreateUserRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}