using MyTourDataViewer.Api.Entities;

namespace MyTourDataViewer.Api.Services;

/// <summary>
/// Provides a ready-to-use Bearer token obtained from the first available
/// External API Settings record. Use this as the default authorization
/// pattern for integration endpoints that do not require manual settings selection.
/// </summary>
public interface IDefaultApiAuthorizationProvider
{
    /// <summary>
    /// Retrieves a Bearer token using the first available <see cref="ApiSettings"/> record.
    /// Returns <c>null</c> for the token when no settings are configured or authorization fails.
    /// </summary>
    Task<DefaultApiAuthorizationResult> GetDefaultAuthorizationAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>Encapsulates the result of a default authorization attempt.</summary>
public sealed class DefaultApiAuthorizationResult
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public ApiSettings? Settings { get; init; }
    public string? ErrorMessage { get; init; }

    public static DefaultApiAuthorizationResult Succeeded(string token, ApiSettings settings) =>
        new() { Success = true, Token = token, Settings = settings };

    public static DefaultApiAuthorizationResult Failed(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };
}
