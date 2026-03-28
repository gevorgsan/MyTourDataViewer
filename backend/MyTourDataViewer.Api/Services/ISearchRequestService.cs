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

    /// <summary>
    /// Calls the external GetRequestHistory endpoint and returns history entries for the given request,
    /// ordered by <c>changedAt</c> descending, with <c>oldValuesJson</c> and <c>newValuesJson</c> parsed
    /// into structured dictionaries.
    /// </summary>
    Task<IList<RequestHistoryItemDto>> GetRequestHistoryAsync(
        int requestId,
        CancellationToken cancellationToken = default);
}
