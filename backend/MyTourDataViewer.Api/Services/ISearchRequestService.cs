using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Services;

public interface ISearchRequestService
{
    /// <summary>
    /// Calls the external SearchRequest endpoint using the Bearer token obtained
    /// from the first available External API Settings record and returns the deserialized results.
    /// </summary>
    Task<IList<SearchRequestItem>> SearchAsync(
        SearchRequestDto request,
        CancellationToken cancellationToken = default);
}
