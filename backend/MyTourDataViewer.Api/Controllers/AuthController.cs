using Microsoft.AspNetCore.Mvc;
using MyTourDataViewer.Api.Models;

namespace MyTourDataViewer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>Authenticate and receive a JWT token.</summary>
    [HttpPost("login")]
    public Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // TODO: implement login
        throw new NotImplementedException();
    }

    /// <summary>Return the profile of the currently authenticated user.</summary>
    [HttpGet("me")]
    public Task<ActionResult<UserDto>> Me()
    {
        // TODO: implement
        throw new NotImplementedException();
    }
}
