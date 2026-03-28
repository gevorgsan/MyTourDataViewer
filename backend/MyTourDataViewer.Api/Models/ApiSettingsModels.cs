using System.ComponentModel.DataAnnotations;
using MyTourDataViewer.Api.Entities;

namespace MyTourDataViewer.Api.Models;

public class ApiEndpointHeaderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ApiEndpointDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "GET";
    public bool RequiresAuthorization { get; set; }
    public AuthorizationType AuthorizationType { get; set; } = AuthorizationType.None;
    public string? TokenEndpointUrl { get; set; }
    public string? Username { get; set; }
    public string? ClientId { get; set; }
    public IList<ApiEndpointHeaderDto> Headers { get; set; } = [];
}

public class ApiEndpointUpsertRequest
{
    public int? Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Url { get; set; } = string.Empty;
    [Required] public string HttpMethod { get; set; } = "GET";
    public bool RequiresAuthorization { get; set; }
    public AuthorizationType AuthorizationType { get; set; } = AuthorizationType.None;
    public string? TokenEndpointUrl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public IList<ApiEndpointHeaderUpsertRequest> Headers { get; set; } = [];
}

public class ApiEndpointHeaderUpsertRequest
{
    public int? Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Value { get; set; } = string.Empty;
}

public class ApiSettingsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>Deprecated: Use endpoint-level Url on each entry in <see cref="Endpoints"/> instead.</summary>
    public string BaseUrl { get; set; } = string.Empty;
    public IList<ApiEndpointDto> Endpoints { get; set; } = [];
    public int TimeoutSeconds { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Authorization configuration ──────────────────────────────────────
    public AuthorizationType AuthorizationType { get; set; }
    public string? TokenUrl { get; set; }
    public string? CredentialsPayload { get; set; }
}

public class CreateApiSettingsRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    /// <summary>Deprecated: Each endpoint now stores its own full URL. Ignored by the server.</summary>
    public string? BaseUrl { get; set; }
    public IList<ApiEndpointUpsertRequest> Endpoints { get; set; } = [];
    [Range(0, 300)] public int TimeoutSeconds { get; set; } = 30;

    // ── Authorization configuration ──────────────────────────────────────
    public AuthorizationType AuthorizationType { get; set; } = AuthorizationType.None;
    public string? TokenUrl { get; set; }
    /// <summary>Raw JSON credentials payload sent to <see cref="TokenUrl"/> to obtain an access token.</summary>
    public string? CredentialsPayload { get; set; }
}

public class UpdateApiSettingsRequest
{
    public string? Name { get; set; }
    /// <summary>Deprecated: Each endpoint now stores its own full URL. Ignored by the server.</summary>
    public string? BaseUrl { get; set; }
    public IList<ApiEndpointUpsertRequest>? Endpoints { get; set; }
    [Range(0, 300)] public int? TimeoutSeconds { get; set; }
    public bool? IsActive { get; set; }

    // ── Authorization configuration ──────────────────────────────────────
    public AuthorizationType? AuthorizationType { get; set; }
    public string? TokenUrl { get; set; }
    /// <summary>Raw JSON credentials payload sent to TokenUrl to obtain an access token.</summary>
    public string? CredentialsPayload { get; set; }
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
