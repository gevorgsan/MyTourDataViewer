using System.Text.Json;
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
        var entries = await _db.ApiSettings
            .AsNoTracking()
            .Include(a => a.Endpoints)
            .ThenInclude(e => e.Headers)
            .ToListAsync();

        return entries.Select(MapToDto);
    }

    public async Task<ApiSettingsDto?> GetByIdAsync(int id)
    {
        var entry = await _db.ApiSettings
            .AsNoTracking()
            .Include(a => a.Endpoints)
            .ThenInclude(e => e.Headers)
            .FirstOrDefaultAsync(a => a.Id == id);

        return entry == null ? null : MapToDto(entry);
    }

    public async Task<ApiSettingsDto> CreateAsync(CreateApiSettingsRequest request)
    {
        var entry = new ApiSettings
        {
            Name           = request.Name,
            BaseUrl        = request.BaseUrl,
            EndpointUrls   = SerializeEndpointUrls(request.Endpoints),
            Endpoints      = request.Endpoints.Select(MapEndpoint).ToList(),
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
        var entry = await _db.ApiSettings
            .Include(a => a.Endpoints)
            .ThenInclude(e => e.Headers)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entry == null) return (false, "Not found.");

        if (request.Name          != null)  entry.Name          = request.Name;
        if (request.BaseUrl       != null)  entry.BaseUrl       = request.BaseUrl;
        if (request.TimeoutSeconds.HasValue) entry.TimeoutSeconds = request.TimeoutSeconds.Value;
        if (request.IsActive.HasValue)      entry.IsActive      = request.IsActive.Value;

        if (request.Endpoints != null)
        {
            _db.ApiEndpointHeaders.RemoveRange(entry.Endpoints.SelectMany(e => e.Headers));
            _db.ApiEndpointSettings.RemoveRange(entry.Endpoints);
            entry.Endpoints = request.Endpoints.Select(MapEndpoint).ToList();
            entry.EndpointUrls = SerializeEndpointUrls(request.Endpoints);
        }

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
        Endpoints      = e.Endpoints.Select(MapEndpointDto).ToList(),
        TimeoutSeconds = e.TimeoutSeconds,
        IsActive       = e.IsActive,
        CreatedAt      = e.CreatedAt,
        UpdatedAt      = e.UpdatedAt
    };

    private static ApiEndpointSettings MapEndpoint(ApiEndpointUpsertRequest request) => new()
    {
        Name = request.Name,
        Url = request.Url,
        HttpMethod = request.HttpMethod,
        RequiresAuthorization = request.RequiresAuthorization,
        AuthorizationType = request.AuthorizationType,
        TokenEndpointUrl = request.TokenEndpointUrl,
        Username = request.Username,
        Password = request.Password,
        ClientId = request.ClientId,
        ClientSecret = request.ClientSecret,
        Headers = request.Headers.Select(MapHeader).ToList()
    };

    private static ApiEndpointHeader MapHeader(ApiEndpointHeaderUpsertRequest request) => new()
    {
        Name = request.Name,
        Value = request.Value
    };

    private static ApiEndpointDto MapEndpointDto(ApiEndpointSettings endpoint) => new()
    {
        Id = endpoint.Id,
        Name = endpoint.Name,
        Url = endpoint.Url,
        HttpMethod = endpoint.HttpMethod,
        RequiresAuthorization = endpoint.RequiresAuthorization,
        AuthorizationType = endpoint.AuthorizationType,
        TokenEndpointUrl = endpoint.TokenEndpointUrl,
        Username = endpoint.Username,
        ClientId = endpoint.ClientId,
        Headers = endpoint.Headers.Select(MapHeaderDto).ToList()
    };

    private static ApiEndpointHeaderDto MapHeaderDto(ApiEndpointHeader header) => new()
    {
        Id = header.Id,
        Name = header.Name,
        Value = header.Value
    };

    private static string SerializeEndpointUrls(IEnumerable<ApiEndpointUpsertRequest> endpoints)
    {
        var urls = endpoints.Select(e => e.Url).ToArray();
        return JsonSerializer.Serialize(urls);
    }
}
