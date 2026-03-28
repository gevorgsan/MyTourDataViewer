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

    /// <summary>Search requests via the external API using the specified API settings for authorization.</summary>
    [HttpPost]
    public async Task<IActionResult> Search(
        [FromQuery] int apiSettingsId,
        [FromBody] SearchRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var results = await _searchRequestService.SearchAsync(apiSettingsId, request, cancellationToken);
            return Ok(results);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "SearchRequest configuration error for API settings {ApiSettingsId}.", apiSettingsId);
            return BadRequest(new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "SearchRequest external API error for API settings {ApiSettingsId}.", apiSettingsId);
            return StatusCode(502, new { message = ex.Message });
        }
    }
}
