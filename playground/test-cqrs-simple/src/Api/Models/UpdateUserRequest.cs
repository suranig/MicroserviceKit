using System.ComponentModel.DataAnnotations;

namespace SimpleService.Api.Models;

/// <summary>
/// Request model for updating User
/// </summary>
public class UpdateUserRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}