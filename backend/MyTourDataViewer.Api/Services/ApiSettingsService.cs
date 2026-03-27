using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>Manages external API configuration entries. Implementation to be added.</summary>
public class ApiSettingsService : IApiSettingsService
{
    private readonly ILogger<ApiSettingsService> _logger;

    public ApiSettingsService(ILogger<ApiSettingsService> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<ApiSettingsDto>> GetAllAsync()
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    public Task<ApiSettingsDto?> GetByIdAsync(int id)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    public Task<ApiSettingsDto> CreateAsync(CreateApiSettingsRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    public Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateApiSettingsRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    public Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        // TODO: implement
        throw new NotImplementedException();
    }
}
