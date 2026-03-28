using System.Text.Json.Serialization;

namespace MyTourDataViewer.Api.Models;

public class ExternalApiTokenCacheEntry
{
    public string Token { get; init; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; init; }
}

public class ExternalApiTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
}

public class ExternalApiAuthorizationResult
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public string? ErrorMessage { get; init; }
    public ExternalApiTokenResponse? Response { get; init; }

    public static ExternalApiAuthorizationResult Succeeded(string token, ExternalApiTokenResponse response) => new()
    {
        Success = true,
        Token = token,
        Response = response
    };

    public static ExternalApiAuthorizationResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}