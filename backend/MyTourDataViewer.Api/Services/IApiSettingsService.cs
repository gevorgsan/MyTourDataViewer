using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

public interface IApiSettingsService
{
    Task<IEnumerable<ApiSettingsDto>> GetAllAsync();
    Task<ApiSettingsDto?> GetByIdAsync(int id);
    Task<ApiSettingsDto> CreateAsync(CreateApiSettingsRequest request);
    Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateApiSettingsRequest request);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}
