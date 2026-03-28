using MyTourDataViewer.Api.Entities;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

public interface IExternalApiAuthorizationService
{
    /// <summary>
    /// Obtains a bearer token using the authorization settings stored on <paramref name="apiSettings"/>
    /// (<see cref="Entities.ApiSettings.TokenUrl"/> and <see cref="Entities.ApiSettings.CredentialsPayload"/>).
    /// </summary>
    Task<ExternalApiAuthorizationResult> GetBearerTokenAsync(
        ApiSettings apiSettings,
        CancellationToken cancellationToken = default);

    /// <summary>Obtains a bearer token for a specific endpoint. Kept for backward compatibility.</summary>
    Task<ExternalApiAuthorizationResult> GetBearerTokenAsync(
        ApiSettings apiSettings,
        ApiEndpointSettings endpoint,
        CancellationToken cancellationToken = default);
}