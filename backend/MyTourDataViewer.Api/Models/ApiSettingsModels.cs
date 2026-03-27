using System.ComponentModel.DataAnnotations;

namespace MyTourDataViewer.Api.Models;

public class ApiSettingsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string EndpointUrls { get; set; } = "[]";
    public string AuthType { get; set; } = "None";
    public string? Username { get; set; }
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateApiSettingsRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required, Url] public string BaseUrl { get; set; } = string.Empty;
    public string EndpointUrls { get; set; } = "[]";
    public string AuthType { get; set; } = "None";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ApiKey { get; set; }
    public string? BearerToken { get; set; }
    [Range(0, 300)] public int TimeoutSeconds { get; set; } = 30;
}

public class UpdateApiSettingsRequest
{
    public string? Name { get; set; }
    public string? BaseUrl { get; set; }
    public string? EndpointUrls { get; set; }
    public string? AuthType { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ApiKey { get; set; }
    public string? BearerToken { get; set; }
    [Range(0, 300)] public int? TimeoutSeconds { get; set; }
    public bool? IsActive { get; set; }
}

public class TestConnectionRequest
{
    public int ApiSettingsId { get; set; }
    public string? EndpointPath { get; set; }
}

public class TestConnectionResponse
{
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public string? Message { get; set; }
    public string? ResponseBody { get; set; }
}
