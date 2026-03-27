namespace MyTourDataViewer.Api.Entities;

public class ApiSettings
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>JSON array of endpoint path strings.</summary>
    public string EndpointUrls { get; set; } = "[]";

    /// <summary>None | ApiKey | Basic | Bearer</summary>
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
}
