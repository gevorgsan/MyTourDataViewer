using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

/// <summary>Communicates with configured external APIs. Implementation to be added.</summary>
public class ExternalApiClientService : IExternalApiClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExternalApiClientService> _logger;

    public ExternalApiClientService(IHttpClientFactory httpClientFactory, ILogger<ExternalApiClientService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Task<TestConnectionResponse> TestConnectionAsync(int apiSettingsId, string? endpointPath)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    public Task<string?> FetchDataAsync(int apiSettingsId, string endpointPath)
    {
        // TODO: implement
        throw new NotImplementedException();
    }
}
