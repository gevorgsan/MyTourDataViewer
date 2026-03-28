using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyTourDataViewer.Api.Data;
using MyTourDataViewer.Api.Entities;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>
/// Calls the external SearchRequest endpoint on behalf of the frontend,
/// obtaining a Bearer token via <see cref="IExternalApiAuthorizationService"/>.
/// </summary>
public class SearchRequestService : ISearchRequestService
{
    private const string DefaultSearchRequestUrl = "https://api.mytour.am/api/Request/SearchRequest";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IExternalApiAuthorizationService _authService;
    private readonly AppDbContext _db;
    private readonly ILogger<SearchRequestService> _logger;

    public SearchRequestService(
        IHttpClientFactory httpClientFactory,
        IExternalApiAuthorizationService authService,
        AppDbContext db,
        ILogger<SearchRequestService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IList<SearchRequestItem>> SearchAsync(
        int apiSettingsId,
        SearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var settings = await _db.ApiSettings
            .AsNoTracking()
            .Include(a => a.Endpoints)
            .FirstOrDefaultAsync(a => a.Id == apiSettingsId, cancellationToken);

        if (settings == null)
        {
            _logger.LogWarning("API settings {ApiSettingsId} not found.", apiSettingsId);
            throw new InvalidOperationException($"API configuration with id {apiSettingsId} was not found.");
        }

        var authResult = await _authService.GetBearerTokenAsync(settings, cancellationToken);
        if (!authResult.Success || string.IsNullOrWhiteSpace(authResult.Token))
        {
            _logger.LogWarning(
                "Failed to retrieve bearer token for API settings {ApiSettingsId}: {Error}",
                apiSettingsId,
                authResult.ErrorMessage);
            throw new InvalidOperationException(
                authResult.ErrorMessage ?? "Failed to retrieve bearer token.");
        }

        var searchRequestUrl = ResolveSearchRequestUrl(settings);

        using var client = _httpClientFactory.CreateClient();
        client.Timeout = settings.TimeoutSeconds > 0
            ? TimeSpan.FromSeconds(settings.TimeoutSeconds)
            : TimeSpan.FromSeconds(30);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, searchRequestUrl)
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);

        _logger.LogInformation(
            "Sending SearchRequest to {Url} using API settings {ApiSettingsId}.",
            searchRequestUrl,
            apiSettingsId);

        using var response = await client.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "SearchRequest failed. StatusCode: {StatusCode}, Response: {ResponseBody}",
                (int)response.StatusCode,
                responseBody.Length > 500 ? responseBody[..500] + "..." : responseBody);
            throw new HttpRequestException(
                $"External API returned status code {(int)response.StatusCode}.");
        }

        IList<SearchRequestItem>? items;
        try
        {
            items = JsonSerializer.Deserialize<IList<SearchRequestItem>>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "SearchRequest returned invalid JSON. Response: {ResponseBody}",
                responseBody.Length > 500 ? responseBody[..500] + "..." : responseBody);
            throw new HttpRequestException("External API returned invalid JSON response.");
        }

        _logger.LogInformation(
            "SearchRequest succeeded. Returned {Count} item(s).",
            items?.Count ?? 0);

        return items ?? [];
    }

    private static string ResolveSearchRequestUrl(ApiSettings settings)
    {
        var namedEndpoint = settings.Endpoints
            .FirstOrDefault(e =>
                !string.IsNullOrWhiteSpace(e.Url) &&
                e.Name.Contains("searchrequest", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(namedEndpoint?.Url))
        {
            return namedEndpoint.Url;
        }

        var firstUrl = settings.Endpoints
            .FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Url))?.Url;

        return !string.IsNullOrWhiteSpace(firstUrl)
            ? firstUrl
            : DefaultSearchRequestUrl;
    }
}
