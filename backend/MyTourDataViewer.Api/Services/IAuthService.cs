using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string username, string password);
}
