using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using MyTourDataViewer.Api.Entities;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>Obtains authorization tokens for external API endpoints.</summary>
public class ExternalApiAuthorizationService : IExternalApiAuthorizationService
{
    private static readonly TimeSpan ExpirationSafetyWindow = TimeSpan.FromSeconds(30);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExternalApiAuthorizationService> _logger;

    public ExternalApiAuthorizationService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<ExternalApiAuthorizationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ExternalApiAuthorizationResult> GetBearerTokenAsync(
        ApiSettings apiSettings,
        CancellationToken cancellationToken = default)
    {
        if (apiSettings.AuthorizationType != AuthorizationType.Bearer)
        {
            _logger.LogWarning(
                "Authorization type {AuthorizationType} is not supported by {ServiceName} for settings {ApiSettingsId}.",
                apiSettings.AuthorizationType,
                nameof(ExternalApiAuthorizationService),
                apiSettings.Id);

            return ExternalApiAuthorizationResult.Failed(
                $"Authorization type '{apiSettings.AuthorizationType}' is not supported.");
        }

        if (string.IsNullOrWhiteSpace(apiSettings.TokenUrl))
        {
            _logger.LogWarning("Token URL is missing for API settings {ApiSettingsId}.", apiSettings.Id);
            return ExternalApiAuthorizationResult.Failed("Token URL is required.");
        }

        if (string.IsNullOrWhiteSpace(apiSettings.CredentialsPayload))
        {
            _logger.LogWarning("Credentials payload is missing for API settings {ApiSettingsId}.", apiSettings.Id);
            return ExternalApiAuthorizationResult.Failed("Credentials payload is required.");
        }

        var cacheKey = $"external-api-token:{apiSettings.Id}";
        if (_cache.TryGetValue<ExternalApiTokenCacheEntry>(cacheKey, out var cachedEntry))
        {
            if (cachedEntry != null && (!cachedEntry.ExpiresAt.HasValue || cachedEntry.ExpiresAt.Value > DateTimeOffset.UtcNow))
            {
                _logger.LogInformation(
                    "Using cached bearer token for API settings {ApiSettingsId}.",
                    apiSettings.Id);

                return ExternalApiAuthorizationResult.Succeeded(
                    cachedEntry.Token,
                    new ExternalApiTokenResponse
                    {
                        AccessToken = cachedEntry.Token,
                        ExpiresIn = cachedEntry.ExpiresAt.HasValue
                            ? (int)Math.Max(0, (cachedEntry.ExpiresAt.Value - DateTimeOffset.UtcNow).TotalSeconds)
                            : null
                    });
            }

            if (cachedEntry != null)
            {
                _logger.LogInformation(
                    "Cached bearer token expired for API settings {ApiSettingsId}. Requesting a new token.",
                    apiSettings.Id);
                _cache.Remove(cacheKey);
            }
        }

        JsonElement credentialsElement;
        try
        {
            if (apiSettings.CredentialsPayload.Length > 8192)
            {
                _logger.LogWarning("Credentials payload exceeds maximum allowed size for API settings {ApiSettingsId}.", apiSettings.Id);
                return ExternalApiAuthorizationResult.Failed("Credentials payload exceeds maximum allowed size.");
            }

            credentialsElement = JsonSerializer.Deserialize<JsonElement>(apiSettings.CredentialsPayload);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid credentials payload JSON for API settings {ApiSettingsId}.", apiSettings.Id);
            return ExternalApiAuthorizationResult.Failed("Credentials payload contains invalid JSON.");
        }

        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = apiSettings.TimeoutSeconds > 0
                ? TimeSpan.FromSeconds(apiSettings.TimeoutSeconds)
                : TimeSpan.FromSeconds(30);

            using var request = new HttpRequestMessage(HttpMethod.Post, apiSettings.TokenUrl)
            {
                Content = JsonContent.Create(credentialsElement)
            };

            _logger.LogInformation(
                "Requesting bearer token for API settings {ApiSettingsId} from {TokenUrl}.",
                apiSettings.Id,
                apiSettings.TokenUrl);

            using var response = await client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Token request failed for API settings {ApiSettingsId}. StatusCode: {StatusCode}, Response: {ResponseBody}",
                    apiSettings.Id,
                    (int)response.StatusCode,
                    Truncate(responseBody));

                return ExternalApiAuthorizationResult.Failed(
                    $"Token request failed with status code {(int)response.StatusCode}.");
            }

            var tokenResponse = JsonSerializer.Deserialize<ExternalApiTokenResponse>(responseBody);
            var token = tokenResponse?.AccessToken ?? tokenResponse?.Token;

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning(
                    "Token response for API settings {ApiSettingsId} did not contain an access token. Response: {ResponseBody}",
                    apiSettings.Id,
                    Truncate(responseBody));

                return ExternalApiAuthorizationResult.Failed("Token response did not contain an access token.");
            }

            var expiresAt = ResolveExpiration(tokenResponse, token);
            CacheToken(cacheKey, token, expiresAt, apiSettings.Id, apiSettings.Name);

            _logger.LogInformation("Successfully retrieved bearer token for API settings {ApiSettingsId}.", apiSettings.Id);
            return ExternalApiAuthorizationResult.Succeeded(token, tokenResponse!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve bearer token for API settings {ApiSettingsId}.", apiSettings.Id);
            return ExternalApiAuthorizationResult.Failed(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<ExternalApiAuthorizationResult> GetBearerTokenAsync(
        ApiSettings apiSettings,
        ApiEndpointSettings endpoint,
        CancellationToken cancellationToken = default)
    {
        if (!endpoint.RequiresAuthorization)
        {
            _logger.LogDebug("Endpoint {EndpointName} does not require authorization.", endpoint.Name);
            return ExternalApiAuthorizationResult.Failed("Authorization is not required for this endpoint.");
        }

        if (endpoint.AuthorizationType != AuthorizationType.Bearer)
        {
            _logger.LogWarning(
                "Authorization type {AuthorizationType} is not supported by {ServiceName} for endpoint {EndpointName}.",
                endpoint.AuthorizationType,
                nameof(ExternalApiAuthorizationService),
                endpoint.Name);

            return ExternalApiAuthorizationResult.Failed(
                $"Authorization type '{endpoint.AuthorizationType}' is not supported.");
        }

        if (string.IsNullOrWhiteSpace(endpoint.TokenEndpointUrl))
        {
            _logger.LogWarning("Token endpoint URL is missing for endpoint {EndpointName}.", endpoint.Name);
            return ExternalApiAuthorizationResult.Failed("Token endpoint URL is required.");
        }

        var cacheKey = BuildCacheKey(apiSettings, endpoint);
        if (_cache.TryGetValue<ExternalApiTokenCacheEntry>(cacheKey, out var cachedEntry))
        {
            if (cachedEntry != null && (!cachedEntry.ExpiresAt.HasValue || cachedEntry.ExpiresAt.Value > DateTimeOffset.UtcNow))
            {
                _logger.LogInformation(
                    "Using cached bearer token for API configuration {ApiSettingsId}, endpoint {EndpointName}.",
                    apiSettings.Id,
                    endpoint.Name);

                return ExternalApiAuthorizationResult.Succeeded(
                    cachedEntry.Token,
                    new ExternalApiTokenResponse
                    {
                        AccessToken = cachedEntry.Token,
                        ExpiresIn = cachedEntry.ExpiresAt.HasValue
                            ? (int)Math.Max(0, (cachedEntry.ExpiresAt.Value - DateTimeOffset.UtcNow).TotalSeconds)
                            : null
                    });
            }

            if (cachedEntry != null)
            {
                _logger.LogInformation(
                    "Cached bearer token expired for API configuration {ApiSettingsId}, endpoint {EndpointName}. Requesting a new token.",
                    apiSettings.Id,
                    endpoint.Name);
                _cache.Remove(cacheKey);
            }
        }

        var requestBody = BuildCredentialsPayload(endpoint);
        if (requestBody.Count == 0)
        {
            _logger.LogWarning("No credentials were configured for endpoint {EndpointName}.", endpoint.Name);
            return ExternalApiAuthorizationResult.Failed("No credentials configured for token retrieval.");
        }

        var tokenUrl = BuildUrl(apiSettings.BaseUrl, endpoint.TokenEndpointUrl);

        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = apiSettings.TimeoutSeconds > 0
                ? TimeSpan.FromSeconds(apiSettings.TimeoutSeconds)
                : TimeSpan.FromSeconds(30);

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = JsonContent.Create(requestBody)
            };

            foreach (var header in endpoint.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Name, header.Value);
            }

            _logger.LogInformation(
                "Requesting bearer token for endpoint {EndpointName} from {TokenUrl}.",
                endpoint.Name,
                tokenUrl);

            using var response = await client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Token request failed for endpoint {EndpointName}. StatusCode: {StatusCode}, Response: {ResponseBody}",
                    endpoint.Name,
                    (int)response.StatusCode,
                    Truncate(responseBody));

                return ExternalApiAuthorizationResult.Failed(
                    $"Token request failed with status code {(int)response.StatusCode}.");
            }

            var tokenResponse = JsonSerializer.Deserialize<ExternalApiTokenResponse>(responseBody);
            var token = tokenResponse?.AccessToken ?? tokenResponse?.Token;

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning(
                    "Token response for endpoint {EndpointName} did not contain an access token. Response: {ResponseBody}",
                    endpoint.Name,
                    Truncate(responseBody));

                return ExternalApiAuthorizationResult.Failed("Token response did not contain an access token.");
            }

            var expiresAt = ResolveExpiration(tokenResponse, token);
            CacheToken(cacheKey, token, expiresAt, apiSettings.Id, endpoint.Name);

            _logger.LogInformation("Successfully retrieved bearer token for endpoint {EndpointName}.", endpoint.Name);
            return ExternalApiAuthorizationResult.Succeeded(token, tokenResponse!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve bearer token for endpoint {EndpointName}.", endpoint.Name);
            return ExternalApiAuthorizationResult.Failed(ex.Message);
        }
    }

    private static Dictionary<string, string> BuildCredentialsPayload(ApiEndpointSettings endpoint)
    {
        var payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(endpoint.Username))
        {
            payload["email"] = endpoint.Username;
        }

        if (!string.IsNullOrWhiteSpace(endpoint.Password))
        {
            payload["password"] = endpoint.Password;
        }

        return payload;
    }

    private static string BuildUrl(string baseUrl, string tokenEndpointUrl)
    {
        if (Uri.TryCreate(tokenEndpointUrl, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        return $"{baseUrl.TrimEnd('/')}/{tokenEndpointUrl.TrimStart('/')}";
    }

    private static string Truncate(string value)
    {
        return value.Length <= 500 ? value : value[..500] + "...";
    }

    private void CacheToken(string cacheKey, string token, DateTimeOffset? expiresAt, int apiSettingsId, string endpointName)
    {
        var cacheEntry = new ExternalApiTokenCacheEntry
        {
            Token = token,
            ExpiresAt = expiresAt
        };

        var options = new MemoryCacheEntryOptions();
        if (expiresAt.HasValue)
        {
            options.AbsoluteExpiration = expiresAt;
        }

        _cache.Set(cacheKey, cacheEntry, options);

        _logger.LogInformation(
            "Cached bearer token for API configuration {ApiSettingsId}, endpoint {EndpointName}. ExpiresAt: {ExpiresAt}",
            apiSettingsId,
            endpointName,
            expiresAt);
    }

    private static DateTimeOffset? ResolveExpiration(ExternalApiTokenResponse? tokenResponse, string token)
    {
        if (tokenResponse?.ExpiresIn is > 0)
        {
            return DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value) - ExpirationSafetyWindow;
        }

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            if (jwt.ValidTo != DateTime.MinValue)
            {
                return new DateTimeOffset(jwt.ValidTo, TimeSpan.Zero) - ExpirationSafetyWindow;
            }
        }
        catch
        {
            // Token is not a JWT or could not be parsed; skip expiration inference.
        }

        return null;
    }

    private static string BuildCacheKey(ApiSettings apiSettings, ApiEndpointSettings endpoint)
    {
        return $"external-api-token:{apiSettings.Id}:{endpoint.Id}";
    }
}