namespace MyTourDataViewer.Api.Entities;

public class ApiEndpointSettings
{
    public int Id { get; set; }
    public int ApiSettingsId { get; set; }
    public ApiSettings ApiSettings { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "GET";

    public bool RequiresAuthorization { get; set; }
    public AuthorizationType AuthorizationType { get; set; } = AuthorizationType.None;

    public string? TokenEndpointUrl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ApiKey { get; set; }

    public ICollection<ApiEndpointHeader> Headers { get; set; } = [];
}