using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTourDataViewer.Api.Models;
using MyTourDataViewer.Api.Services;

namespace MyTourDataViewer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        var (success, error, user) = await _userService.CreateAsync(request);
        if (!success)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetById), new { id = user!.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var (success, error) = await _userService.UpdateAsync(id, request);
        if (!success)
            return error == "User not found." ? NotFound() : BadRequest(new { message = error });

        return NoContent();
    }

    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
    {
        var (success, error) = await _userService.ChangePasswordAsync(id, request.NewPassword);
        if (!success)
            return error == "User not found." ? NotFound(new { message = error }) : BadRequest(new { message = error });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var (success, error) = await _userService.DeleteAsync(id);
        if (!success)
            return error == "User not found." ? NotFound() : BadRequest(new { message = error });

        return NoContent();
    }
}
