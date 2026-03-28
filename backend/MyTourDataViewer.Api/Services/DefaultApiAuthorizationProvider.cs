using Microsoft.EntityFrameworkCore;
using MyTourDataViewer.Api.Data;
using MyTourDataViewer.Api.Entities;

namespace MyTourDataViewer.Api.Services;

/// <summary>
/// Default implementation of <see cref="IDefaultApiAuthorizationProvider"/>.
/// Fetches the first active <see cref="ApiSettings"/> record from the database
/// and obtains a Bearer token via <see cref="IExternalApiAuthorizationService"/>.
/// Intended as a reusable default authorization pattern for integration endpoints.
/// </summary>
public class DefaultApiAuthorizationProvider : IDefaultApiAuthorizationProvider
{
    private readonly AppDbContext _db;
    private readonly IExternalApiAuthorizationService _authService;
    private readonly ILogger<DefaultApiAuthorizationProvider> _logger;

    public DefaultApiAuthorizationProvider(
        AppDbContext db,
        IExternalApiAuthorizationService authService,
        ILogger<DefaultApiAuthorizationProvider> logger)
    {
        _db = db;
        _authService = authService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<DefaultApiAuthorizationResult> GetDefaultAuthorizationAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await _db.ApiSettings
            .AsNoTracking()
            .Include(a => a.Endpoints)
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            _logger.LogWarning(
                "No External API Settings records are configured. " +
                "Add at least one entry to enable external API authorization.");
            return DefaultApiAuthorizationResult.Failed(
                "No External API Settings are configured.");
        }

        _logger.LogDebug(
            "Using API settings {ApiSettingsId} ({ApiSettingsName}) as default authorization source.",
            settings.Id,
            settings.Name);

        var authResult = await _authService.GetBearerTokenAsync(settings, cancellationToken);

        if (!authResult.Success || string.IsNullOrWhiteSpace(authResult.Token))
        {
            _logger.LogWarning(
                "Default authorization failed for API settings {ApiSettingsId} ({ApiSettingsName}): {Error}",
                settings.Id,
                settings.Name,
                authResult.ErrorMessage);
            return DefaultApiAuthorizationResult.Failed(
                authResult.ErrorMessage ?? "Failed to retrieve bearer token.");
        }

        return DefaultApiAuthorizationResult.Succeeded(authResult.Token, settings);
    }
}
