using System.ComponentModel.DataAnnotations;

namespace MyTourDataViewer.Api.Models;

public class LoginRequest
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = [];
    public DateTime Expiry { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<string> Roles { get; set; } = [];
}

public class CreateUserRequest
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = "Viewer";
}

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? Password { get; set; }
}

public class ChangePasswordRequest
{
    [Required, MinLength(6)] public string NewPassword { get; set; } = string.Empty;
}
