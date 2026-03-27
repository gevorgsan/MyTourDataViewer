using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MyTourDataViewer.Api.Data;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>Communicates with configured external APIs.</summary>
public class ExternalApiClientService : IExternalApiClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppDbContext _db;
    private readonly ILogger<ExternalApiClientService> _logger;

    public ExternalApiClientService(
        IHttpClientFactory httpClientFactory,
        AppDbContext db,
        ILogger<ExternalApiClientService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _db = db;
        _logger = logger;
    }

    public async Task<TestConnectionResponse> TestConnectionAsync(int apiSettingsId, string? endpointPath)
    {
        var settings = await _db.ApiSettings.AsNoTracking().FirstOrDefaultAsync(a => a.Id == apiSettingsId);
        if (settings == null)
            return new TestConnectionResponse { Success = false, Message = "API configuration not found." };

        try
        {
            using var client = BuildClient(settings);
            var url = BuildUrl(settings.BaseUrl, endpointPath);
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            return new TestConnectionResponse
            {
                Success      = response.IsSuccessStatusCode,
                StatusCode   = (int)response.StatusCode,
                Message      = response.IsSuccessStatusCode ? "Connection successful." : response.ReasonPhrase,
                ResponseBody = body.Length > 2000 ? body[..2000] + "…" : body
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Test connection failed for ApiSettings {Id}", apiSettingsId);
            return new TestConnectionResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<string?> FetchDataAsync(int apiSettingsId, string endpointPath)
    {
        var settings = await _db.ApiSettings.AsNoTracking().FirstOrDefaultAsync(a => a.Id == apiSettingsId);
        if (settings == null || !settings.IsActive) return null;

        try
        {
            using var client = BuildClient(settings);
            var url = BuildUrl(settings.BaseUrl, endpointPath);
            return await client.GetStringAsync(url);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FetchData failed for ApiSettings {Id}, endpoint {Endpoint}", apiSettingsId, endpointPath);
            return null;
        }
    }

    private HttpClient BuildClient(Entities.ApiSettings settings)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = settings.TimeoutSeconds > 0
            ? TimeSpan.FromSeconds(settings.TimeoutSeconds)
            : TimeSpan.FromSeconds(30);

        switch (settings.AuthType)
        {
            case "ApiKey" when !string.IsNullOrEmpty(settings.ApiKey):
                client.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);
                break;

            case "Basic" when !string.IsNullOrEmpty(settings.Username):
                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{settings.Username}:{settings.Password}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);
                break;

            case "Bearer" when !string.IsNullOrEmpty(settings.BearerToken):
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", settings.BearerToken);
                break;
        }

        return client;
    }

    private static string BuildUrl(string baseUrl, string? endpointPath)
    {
        if (string.IsNullOrWhiteSpace(endpointPath)) return baseUrl;
        return $"{baseUrl.TrimEnd('/')}/{endpointPath.TrimStart('/')}";
    }
}
