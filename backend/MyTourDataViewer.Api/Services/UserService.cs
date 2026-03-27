using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>Manages application users. Implementation to be added.</summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<UserDto>> GetAllAsync()
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    public Task<UserDto?> GetByIdAsync(string id)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    public Task<(bool Success, string? Error, UserDto? User)> CreateAsync(CreateUserRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    public Task<(bool Success, string? Error)> UpdateAsync(string id, UpdateUserRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    public Task<(bool Success, string? Error)> DeleteAsync(string id)
    {
        // TODO: implement
        throw new NotImplementedException();
    }
}
