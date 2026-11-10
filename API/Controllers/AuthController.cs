using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
namespace IRacingLeague.API.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDtoIn dto, CancellationToken cancellationToken) => Ok(new { token = await auth.RegisterAsync(dto, cancellationToken) });
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDtoIn dto, CancellationToken cancellationToken) => Ok(new { token = await auth.LoginAsync(dto, cancellationToken) });
}
