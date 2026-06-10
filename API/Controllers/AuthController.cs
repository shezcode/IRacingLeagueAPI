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
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var token = _auth.Register(dto);
            return Ok(new { token });
        }
        catch (InvalidOperationException ex)
        {
            // Duplicate email, client error, not server fault.
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", dto.Email);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDtoIn dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var token = _auth.Login(dto);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {Email}", dto.Email);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
