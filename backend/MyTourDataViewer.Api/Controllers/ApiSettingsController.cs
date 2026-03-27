using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class ApiSettingsController : ControllerBase
{
    private readonly ILogger<ApiSettingsController> _logger;

    public ApiSettingsController(ILogger<ApiSettingsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public Task<ActionResult<IEnumerable<ApiSettingsDto>>> GetAll()
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpGet("{id:int}")]
    public Task<ActionResult<ApiSettingsDto>> GetById(int id)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpPost]
    public Task<ActionResult<ApiSettingsDto>> Create([FromBody] CreateApiSettingsRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpPut("{id:int}")]
    public Task<IActionResult> Update(int id, [FromBody] UpdateApiSettingsRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpDelete("{id:int}")]
    public Task<IActionResult> Delete(int id)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpPost("test")]
    public Task<ActionResult<TestConnectionResponse>> TestConnection([FromBody] TestConnectionRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }
}
