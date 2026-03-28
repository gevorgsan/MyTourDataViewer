namespace MyTourDataViewer.Api.Entities;

public class ApiEndpointHeader
{
    public int Id { get; set; }
    public int ApiEndpointSettingsId { get; set; }
    public ApiEndpointSettings ApiEndpointSettings { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}