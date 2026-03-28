using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

public interface ISearchRequestService
{
    /// <summary>
    /// Calls the external SearchRequest endpoint using the Bearer token obtained
    /// from the specified API settings entry and returns the deserialized results.
    /// </summary>
    Task<IList<SearchRequestItem>> SearchAsync(
        int apiSettingsId,
        SearchRequestDto request,
        CancellationToken cancellationToken = default);
}
