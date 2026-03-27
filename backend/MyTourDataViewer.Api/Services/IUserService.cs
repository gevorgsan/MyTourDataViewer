using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(string id);
    Task<(bool Success, string? Error, UserDto? User)> CreateAsync(CreateUserRequest request);
    Task<(bool Success, string? Error)> UpdateAsync(string id, UpdateUserRequest request);
    Task<(bool Success, string? Error)> DeleteAsync(string id);
}
