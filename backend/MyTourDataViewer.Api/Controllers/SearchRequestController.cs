using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTourDataViewer.Api.Models;
using MyTourDataViewer.Api.Services;

namespace MyTourDataViewer.Api.Controllers;

[ApiController]
[Route("api/searchrequest")]
[Authorize]
public class SearchRequestController : ControllerBase
{
    private readonly ISearchRequestService _searchRequestService;
    private readonly ILogger<SearchRequestController> _logger;

    public SearchRequestController(
        ISearchRequestService searchRequestService,
        ILogger<SearchRequestController> logger)
    {
        _searchRequestService = searchRequestService;
        _logger = logger;
    }

    /// <summary>Search requests via the external API using the default API settings for authorization.</summary>
    [HttpPost]
    public async Task<IActionResult> Search(
        [FromBody] SearchRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var results = await _searchRequestService.SearchAsync(request, cancellationToken);
            return Ok(results);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "SearchRequest configuration error.");
            return BadRequest(new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "SearchRequest external API error.");
            return StatusCode(502, new { message = ex.Message });
        }
    }

    /// <summary>Retrieve history for a specific request via the external API.</summary>
    [HttpGet("{requestId:int}/history")]
    public async Task<IActionResult> GetHistory(int requestId, CancellationToken cancellationToken)
    {
        try
        {
            var history = await _searchRequestService.GetRequestHistoryAsync(requestId, cancellationToken);
            return Ok(history);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "GetRequestHistory configuration error.");
            return BadRequest(new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "GetRequestHistory external API error.");
            return StatusCode(502, new { message = ex.Message });
        }
    }
}
