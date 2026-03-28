using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTourDataViewer.Api.Services;

namespace MyTourDataViewer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IApiSettingsService _apiSettingsService;
    private readonly IExternalApiClientService _externalApiClient;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IApiSettingsService apiSettingsService,
        IExternalApiClientService externalApiClient,
        ILogger<DashboardController> logger)
    {
        _apiSettingsService = apiSettingsService;
        _externalApiClient = externalApiClient;
        _logger = logger;
    }

    /// <summary>Fetch data from a configured external API for the dashboard.</summary>
    [HttpGet("data")]
    public async Task<IActionResult> GetData([FromQuery] int apiSettingsId, [FromQuery] string endpoint = "")
    {
        var data = await _externalApiClient.FetchDataAsync(apiSettingsId, endpoint);
        if (data == null)
            return NotFound(new { message = "No data returned or API configuration not found." });
        return Content(data, "application/json");
    }

    /// <summary>Return the list of active external API configurations available to the dashboard.</summary>
    [HttpGet("apis")]
    public async Task<IActionResult> GetAvailableApis()
    {
        var all = await _apiSettingsService.GetAllAsync();
        var active = all.Where(a => a.IsActive).Select(a => new
        {
            a.Id,
            a.Name,
            a.BaseUrl,
            EndpointUrls = JsonSerializer.Serialize(a.Endpoints.Select(e => e.Url))
        });

        return Ok(active);
    }
}
