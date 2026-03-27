using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;

    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpGet("{id}")]
    public Task<ActionResult<UserDto>> GetById(string id)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpPost]
    public Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpPut("{id}")]
    public Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        // TODO: implement
        throw new NotImplementedException();
    }

    [HttpDelete("{id}")]
    public Task<IActionResult> Delete(string id)
    {
        // TODO: implement
        throw new NotImplementedException();
    }
}
