using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, ILogger<AuthController> logger)
    {
        _auth = auth;
        _logger = logger;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserDtoIn dto)
    {

            var token = _auth.Register(dto);
            return Ok(new { token });

    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDtoIn dto)
    {

            var token = _auth.Login(dto);
            return Ok(new { token });

    }
}
