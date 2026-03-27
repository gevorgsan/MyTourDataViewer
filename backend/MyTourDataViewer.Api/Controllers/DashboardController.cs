using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyTourDataViewer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger)
    {
        _logger = logger;
    }

    /// <summary>Fetch data from a configured external API for the dashboard.</summary>
    [HttpGet("data")]
    public Task<IActionResult> GetData([FromQuery] int apiSettingsId, [FromQuery] string endpoint = "")
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    /// <summary>Return the list of active external API configurations available to the dashboard.</summary>
    [HttpGet("apis")]
    public Task<IActionResult> GetAvailableApis()
    {
        // TODO: implement
        throw new NotImplementedException();
    }
}
