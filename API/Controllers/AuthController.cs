using IRacingLeague.Business;
using IRacingLeague.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IRacingLeague.API.Controllers;

/// <summary>Registers and authenticates drivers.</summary>
[ApiController]
[Route("api/v1/auth")]
[Tags("Authentication")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>Registers a driver and returns an access token.</summary>
    /// <remarks>Example request: <code>{"userName":"Ayrton","email":"driver@example.com","password":"long-password","tag":"AYR"}</code></remarks>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] UserDtoIn dto, CancellationToken cancellationToken) =>
        Ok(new { token = await auth.RegisterAsync(dto, cancellationToken) });

    /// <summary>Authenticates a driver and returns an access token.</summary>
    /// <remarks>Example request: <code>{"email":"driver@example.com","password":"long-password"}</code></remarks>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginDtoIn dto, CancellationToken cancellationToken) =>
        Ok(new { token = await auth.LoginAsync(dto, cancellationToken) });
}
