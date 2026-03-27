using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>Handles login and JWT generation. Implementation to be added.</summary>
public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;

    public AuthService(ILogger<AuthService> logger)
    {
        _logger = logger;
    }

    public Task<LoginResponse?> LoginAsync(string username, string password)
    {
        // TODO: implement authentication logic
        throw new NotImplementedException();
    }
}
