using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

public interface IExternalApiClientService
{
    Task<TestConnectionResponse> TestConnectionAsync(int apiSettingsId, string? endpointPath);
    Task<string?> FetchDataAsync(int apiSettingsId, string endpointPath);
}
