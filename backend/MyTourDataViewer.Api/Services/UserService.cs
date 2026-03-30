using Microsoft.AspNetCore.Identity;
using MyTourDataViewer.Api.Entities;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>Manages application users via ASP.NET Core Identity.</summary>
public class UserService : IUserService
{
    private static readonly string[] ValidRoles = ["Administrator", "Viewer"];

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = _userManager.Users.ToList();
        var dtos = new List<UserDto>(users.Count);
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            dtos.Add(MapToDto(u, roles));
        }
        return dtos;
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;
        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task<(bool Success, string? Error, UserDto? User)> CreateAsync(CreateUserRequest request)
    {
        if (!ValidRoles.Contains(request.Role))
            return (false, $"Role must be one of: {string.Join(", ", ValidRoles)}", null);

        var user = new ApplicationUser
        {
            UserName       = request.Username,
            Email          = request.Email,
            FullName       = request.FullName,
            IsActive       = true,
            EmailConfirmed = true,
            CreatedAt      = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user {Username}: {Error}", request.Username, error);
            return (false, error, null);
        }

        await _userManager.AddToRoleAsync(user, request.Role);
        var roles = await _userManager.GetRolesAsync(user);
        return (true, null, MapToDto(user, roles));
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(string id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, "User not found.");

        if (request.FullName != null)  user.FullName = request.FullName;
        if (request.Email    != null)  user.Email    = request.Email;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var error = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            return (false, error);
        }

        if (!string.IsNullOrEmpty(request.Role))
        {
            if (!ValidRoles.Contains(request.Role))
                return (false, $"Role must be one of: {string.Join(", ", ValidRoles)}");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, request.Role);
        }

        if (!string.IsNullOrEmpty(request.Password))
        {
            var token    = await _userManager.GeneratePasswordResetTokenAsync(user);
            var pwResult = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!pwResult.Succeeded)
            {
                var error = string.Join("; ", pwResult.Errors.Select(e => e.Description));
                return (false, error);
            }
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(string id, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, "User not found.");

        var token    = await _userManager.GeneratePasswordResetTokenAsync(user);
        var pwResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!pwResult.Succeeded)
        {
            var error = string.Join("; ", pwResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to change password for user {Id}: {Error}", id, error);
            return (false, error);
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, "User not found.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var error = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, error);
        }

        return (true, null);
    }

    private static UserDto MapToDto(ApplicationUser user, IList<string> roles) => new()
    {
        Id        = user.Id,
        Username  = user.UserName!,
        Email     = user.Email ?? string.Empty,
        FullName  = user.FullName,
        IsActive  = user.IsActive,
        CreatedAt = user.CreatedAt,
        Roles     = roles
    };
}
