namespace MyTourDataViewer.Api.Entities;

public class ApiSettings
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Deprecated: Each endpoint now stores its own full <see cref="ApiEndpointSettings.Url"/>. Kept for backward compatibility only.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    public ICollection<ApiEndpointSettings> Endpoints { get; set; } = [];

    /// <summary>Deprecated: Endpoints are stored as separate <see cref="ApiEndpointSettings"/> records. This field is no longer populated.</summary>
    public string EndpointUrls { get; set; } = "[]";

    /// <summary>Deprecated: None | ApiKey | Basic | Bearer</summary>
    public string AuthType { get; set; } = "None";

    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ApiKey { get; set; }
    public string? BearerToken { get; set; }

    /// <summary>HTTP request timeout in seconds (0 = use default).</summary>
    public int TimeoutSeconds { get; set; } = 30;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Authorization configuration ────────────────────────────────────────────

    /// <summary>External service authorization type (e.g. Bearer token request).</summary>
    public AuthorizationType AuthorizationType { get; set; } = AuthorizationType.None;

    /// <summary>URL of the external service token endpoint.</summary>
    public string? TokenUrl { get; set; }

    /// <summary>Raw JSON credentials payload sent to <see cref="TokenUrl"/> when requesting an access token.
    /// Example: {"email":"user@example.com","password":"secret"}</summary>
    public string? CredentialsPayload { get; set; }
}
