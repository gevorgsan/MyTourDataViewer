using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using MyTourDataViewer.Api.Entities;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>
/// Calls the external SearchRequest endpoint on behalf of the frontend,
/// automatically using the first available External API Settings record for authorization.
/// </summary>
public class SearchRequestService : ISearchRequestService
{
    private const string DefaultSearchRequestUrl = "https://api.mytour.am/api/Request/SearchRequest";
    private const string DefaultRequestHistoryUrl = "https://api.mytour.am/api/Admin/GetRequestHistory";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDefaultApiAuthorizationProvider _authorizationProvider;
    private readonly ILogger<SearchRequestService> _logger;

    public SearchRequestService(
        IHttpClientFactory httpClientFactory,
        IDefaultApiAuthorizationProvider authorizationProvider,
        ILogger<SearchRequestService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _authorizationProvider = authorizationProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IList<SearchRequestItem>> SearchAsync(
        SearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var authResult = await _authorizationProvider.GetDefaultAuthorizationAsync(cancellationToken);
        if (!authResult.Success || string.IsNullOrWhiteSpace(authResult.Token))
        {
            throw new InvalidOperationException(
                authResult.ErrorMessage ?? "Failed to retrieve bearer token.");
        }

        var settings = authResult.Settings!;
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
            settings.Id);

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

    /// <inheritdoc />
    public async Task<IList<RequestHistoryItemDto>> GetRequestHistoryAsync(
        int requestId,
        CancellationToken cancellationToken = default)
    {
        var authResult = await _authorizationProvider.GetDefaultAuthorizationAsync(cancellationToken);
        if (!authResult.Success || string.IsNullOrWhiteSpace(authResult.Token))
        {
            throw new InvalidOperationException(
                authResult.ErrorMessage ?? "Failed to retrieve bearer token.");
        }

        var settings = authResult.Settings!;
        var historyUrl = $"{DefaultRequestHistoryUrl}?requestId={requestId}";

        using var client = _httpClientFactory.CreateClient();
        client.Timeout = settings.TimeoutSeconds > 0
            ? TimeSpan.FromSeconds(settings.TimeoutSeconds)
            : TimeSpan.FromSeconds(30);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, historyUrl);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);

        _logger.LogInformation(
            "Fetching request history for requestId={RequestId} using API settings {ApiSettingsId}.",
            requestId,
            settings.Id);

        using var response = await client.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "GetRequestHistory failed. StatusCode: {StatusCode}, Response: {ResponseBody}",
                (int)response.StatusCode,
                responseBody.Length > 500 ? responseBody[..500] + "..." : responseBody);
            throw new HttpRequestException(
                $"External API returned status code {(int)response.StatusCode}.");
        }

        IList<RequestHistoryItem>? items;
        try
        {
            items = JsonSerializer.Deserialize<IList<RequestHistoryItem>>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "GetRequestHistory returned invalid JSON. Response: {ResponseBody}",
                responseBody.Length > 500 ? responseBody[..500] + "..." : responseBody);
            throw new HttpRequestException("External API returned invalid JSON response.");
        }

        if (items == null || items.Count == 0)
        {
            return [];
        }

        var result = items
            .OrderByDescending(i => i.ChangedAt)
            .Select(i => new RequestHistoryItemDto
            {
                RequestId = i.RequestId,
                ChangeType = i.ChangeType,
                ChangedAt = i.ChangedAt,
                ChangedBy = i.ChangedBy,
                OldValues = ParseJsonToDictionary(i.OldValuesJson),
                NewValues = ParseJsonToDictionary(i.NewValuesJson)
            })
            .ToList();

        _logger.LogInformation(
            "GetRequestHistory succeeded for requestId={RequestId}. Returned {Count} record(s).",
            requestId,
            result.Count);

        return result;
    }

    private static Dictionary<string, object?>? ParseJsonToDictionary(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var node = JsonNode.Parse(json);
            if (node is not JsonObject obj)
                return null;

            var dict = new Dictionary<string, object?>();
            foreach (var kv in obj)
            {
                dict[kv.Key] = kv.Value switch
                {
                    null => null,
                    JsonValue v when v.TryGetValue<bool>(out var b) => b,
                    JsonValue v when v.TryGetValue<long>(out var l) => l,
                    JsonValue v when v.TryGetValue<double>(out var d) => d,
                    JsonValue v when v.TryGetValue<decimal>(out var m) => m,
                    JsonValue v when v.TryGetValue<string>(out var s) => s,
                    _ => kv.Value?.ToJsonString()
                };
            }
            return dict;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
