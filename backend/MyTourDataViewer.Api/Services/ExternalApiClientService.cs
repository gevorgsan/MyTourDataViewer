using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MyTourDataViewer.Api.Data;
using MyTourDataViewer.Api.Entities;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>Communicates with configured external APIs.</summary>
public class ExternalApiClientService : IExternalApiClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IExternalApiAuthorizationService _externalApiAuthorizationService;
    private readonly AppDbContext _db;
    private readonly ILogger<ExternalApiClientService> _logger;

    public ExternalApiClientService(
        IHttpClientFactory httpClientFactory,
        IExternalApiAuthorizationService externalApiAuthorizationService,
        AppDbContext db,
        ILogger<ExternalApiClientService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _externalApiAuthorizationService = externalApiAuthorizationService;
        _db = db;
        _logger = logger;
    }

    public async Task<TestConnectionResponse> TestConnectionAsync(int apiSettingsId, string? endpointPath)
    {
        var settings = await _db.ApiSettings
            .AsNoTracking()
            .Include(a => a.Endpoints)
            .ThenInclude(e => e.Headers)
            .FirstOrDefaultAsync(a => a.Id == apiSettingsId);

        if (settings == null)
            return new TestConnectionResponse { Success = false, Message = "API configuration not found." };

        try
        {
            using var client = BuildClient(settings);
            var endpoint = FindEndpoint(settings, endpointPath);
            var url = endpoint?.Url ?? endpointPath;
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            var authorizationError = await ApplyAuthorizationAsync(request, settings, endpoint);
            if (authorizationError != null)
            {
                return new TestConnectionResponse { Success = false, Message = authorizationError };
            }

            var response = await client.SendAsync(request);
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
        var settings = await _db.ApiSettings
            .AsNoTracking()
            .Include(a => a.Endpoints)
            .ThenInclude(e => e.Headers)
            .FirstOrDefaultAsync(a => a.Id == apiSettingsId);

        if (settings == null || !settings.IsActive) return null;

        try
        {
            using var client = BuildClient(settings);
            var endpoint = FindEndpoint(settings, endpointPath);
            var url = endpoint?.Url ?? endpointPath;
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            var authorizationError = await ApplyAuthorizationAsync(request, settings, endpoint);
            if (authorizationError != null)
            {
                _logger.LogWarning(
                    "Authorization failed for ApiSettings {Id}, endpoint {Endpoint}: {Error}",
                    apiSettingsId,
                    endpointPath,
                    authorizationError);
                return null;
            }

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FetchData failed for ApiSettings {Id}, endpoint {Endpoint}", apiSettingsId, endpointPath);
            return null;
        }
    }

    private async Task<string?> ApplyAuthorizationAsync(
        HttpRequestMessage request,
        ApiSettings settings,
        ApiEndpointSettings? endpoint,
        CancellationToken cancellationToken = default)
    {
        if (endpoint == null || !endpoint.RequiresAuthorization)
        {
            return null;
        }

        _logger.LogInformation(
            "Attempting authorization for external API endpoint {EndpointName} ({EndpointUrl}).",
            endpoint.Name,
            endpoint.Url);

        var authorizationResult = await _externalApiAuthorizationService
            .GetBearerTokenAsync(settings, endpoint, cancellationToken);

        if (!authorizationResult.Success || string.IsNullOrWhiteSpace(authorizationResult.Token))
        {
            var errorMessage = authorizationResult.ErrorMessage ?? "Failed to retrieve bearer token.";
            _logger.LogWarning(
                "Authorization failed for external API endpoint {EndpointName}: {Error}",
                endpoint.Name,
                errorMessage);
            return errorMessage;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationResult.Token);
        return null;
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

    private static ApiEndpointSettings? FindEndpoint(ApiSettings settings, string? endpointPath)
    {
        if (string.IsNullOrWhiteSpace(endpointPath))
        {
            return null;
        }

        return settings.Endpoints.FirstOrDefault(endpoint =>
            string.Equals(NormalizeUrl(endpoint.Url ?? string.Empty), NormalizeUrl(endpointPath), StringComparison.OrdinalIgnoreCase)
            || string.Equals(endpoint.Name, endpointPath, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeUrl(string url)
    {
        return url.Trim().Trim('/');
    }
}
