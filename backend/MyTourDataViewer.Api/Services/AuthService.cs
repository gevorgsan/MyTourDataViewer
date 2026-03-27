using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MyTourDataViewer.Api.Entities;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>Handles login and JWT token generation.</summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _config = config;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Login failed for {Username}: user not found or inactive", username);
            return null;
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            _logger.LogWarning("Login failed for {Username}: invalid password", username);
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var expiryMinutes = GetExpiryMinutes();
        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        return new LoginResponse
        {
            Token = GenerateToken(user, roles, expiry),
            Username = user.UserName!,
            FullName = user.FullName,
            Roles = roles,
            Expiry = expiry
        };
    }

    private string GenerateToken(ApplicationUser user, IList<string> roles, DateTime expiry)
    {
        var key = _config["Jwt:Key"]!;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,        user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new(JwtRegisteredClaimNames.Email,      user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer:            _config["Jwt:Issuer"],
            audience:          _config["Jwt:Audience"],
            claims:            claims,
            expires:           expiry,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private int GetExpiryMinutes() =>
        int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 60;
}
