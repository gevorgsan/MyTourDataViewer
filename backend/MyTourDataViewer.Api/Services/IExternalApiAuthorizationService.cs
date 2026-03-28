using MyTourDataViewer.Api.Entities;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

public interface IExternalApiAuthorizationService
{
    Task<ExternalApiAuthorizationResult> GetBearerTokenAsync(
        ApiSettings apiSettings,
        ApiEndpointSettings endpoint,
        CancellationToken cancellationToken = default);
}