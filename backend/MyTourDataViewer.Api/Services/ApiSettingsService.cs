using Microsoft.EntityFrameworkCore;
using MyTourDataViewer.Api.Data;
using MyTourDataViewer.Api.Entities;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>Manages external API configuration entries.</summary>
public class ApiSettingsService : IApiSettingsService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ApiSettingsService> _logger;

    public ApiSettingsService(AppDbContext db, ILogger<ApiSettingsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<ApiSettingsDto>> GetAllAsync()
    {
        var entries = await _db.ApiSettings.AsNoTracking().ToListAsync();
        return entries.Select(MapToDto);
    }

    public async Task<ApiSettingsDto?> GetByIdAsync(int id)
    {
        var entry = await _db.ApiSettings.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        return entry == null ? null : MapToDto(entry);
    }

    public async Task<ApiSettingsDto> CreateAsync(CreateApiSettingsRequest request)
    {
        var entry = new ApiSettings
        {
            Name           = request.Name,
            BaseUrl        = request.BaseUrl,
            EndpointUrls   = request.EndpointUrls,
            AuthType       = request.AuthType,
            Username       = request.Username,
            Password       = request.Password,
            ApiKey         = request.ApiKey,
            BearerToken    = request.BearerToken,
            TimeoutSeconds = request.TimeoutSeconds,
            IsActive       = true,
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow
        };

        _db.ApiSettings.Add(entry);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created ApiSettings {Id} ({Name})", entry.Id, entry.Name);
        return MapToDto(entry);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateApiSettingsRequest request)
    {
        var entry = await _db.ApiSettings.FindAsync(id);
        if (entry == null) return (false, "Not found.");

        if (request.Name          != null)  entry.Name          = request.Name;
        if (request.BaseUrl       != null)  entry.BaseUrl       = request.BaseUrl;
        if (request.EndpointUrls  != null)  entry.EndpointUrls  = request.EndpointUrls;
        if (request.AuthType      != null)  entry.AuthType      = request.AuthType;
        if (request.Username      != null)  entry.Username      = request.Username;
        if (request.Password      != null)  entry.Password      = request.Password;
        if (request.ApiKey        != null)  entry.ApiKey        = request.ApiKey;
        if (request.BearerToken   != null)  entry.BearerToken   = request.BearerToken;
        if (request.TimeoutSeconds.HasValue) entry.TimeoutSeconds = request.TimeoutSeconds.Value;
        if (request.IsActive.HasValue)      entry.IsActive      = request.IsActive.Value;
        entry.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var entry = await _db.ApiSettings.FindAsync(id);
        if (entry == null) return (false, "Not found.");

        _db.ApiSettings.Remove(entry);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    private static ApiSettingsDto MapToDto(ApiSettings e) => new()
    {
        Id             = e.Id,
        Name           = e.Name,
        BaseUrl        = e.BaseUrl,
        EndpointUrls   = e.EndpointUrls,
        AuthType       = e.AuthType,
        Username       = e.Username,
        ApiKey         = e.ApiKey,
        TimeoutSeconds = e.TimeoutSeconds,
        IsActive       = e.IsActive,
        CreatedAt      = e.CreatedAt,
        UpdatedAt      = e.UpdatedAt
    };
}
