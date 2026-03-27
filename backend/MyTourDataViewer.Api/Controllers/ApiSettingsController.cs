using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTourDataViewer.Api.Models;
using MyTourDataViewer.Api.Services;

namespace MyTourDataViewer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class ApiSettingsController : ControllerBase
{
    private readonly IApiSettingsService _apiSettingsService;
    private readonly IExternalApiClientService _externalApiClient;
    private readonly ILogger<ApiSettingsController> _logger;

    public ApiSettingsController(
        IApiSettingsService apiSettingsService,
        IExternalApiClientService externalApiClient,
        ILogger<ApiSettingsController> logger)
    {
        _apiSettingsService = apiSettingsService;
        _externalApiClient = externalApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApiSettingsDto>>> GetAll()
    {
        var items = await _apiSettingsService.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiSettingsDto>> GetById(int id)
    {
        var item = await _apiSettingsService.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ApiSettingsDto>> Create([FromBody] CreateApiSettingsRequest request)
    {
        var created = await _apiSettingsService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateApiSettingsRequest request)
    {
        var (success, error) = await _apiSettingsService.UpdateAsync(id, request);
        if (!success)
            return error == "Not found." ? NotFound() : BadRequest(new { message = error });
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, error) = await _apiSettingsService.DeleteAsync(id);
        if (!success)
            return error == "Not found." ? NotFound() : BadRequest(new { message = error });
        return NoContent();
    }

    [HttpPost("test")]
    public async Task<ActionResult<TestConnectionResponse>> TestConnection([FromBody] TestConnectionRequest request)
    {
        var result = await _externalApiClient.TestConnectionAsync(request.ApiSettingsId, request.EndpointPath);
        return Ok(result);
    }
}
