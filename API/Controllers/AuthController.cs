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
    /// <summary>Registers a driver and returns access and refresh tokens.</summary>
    /// <remarks>Example request: <code>{"userName":"Ayrton","email":"driver@example.com","password":"long-password","tag":"AYR"}</code></remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] UserDtoIn dto, CancellationToken cancellationToken) =>
        Ok(await auth.RegisterAsync(dto, cancellationToken));

    /// <summary>Authenticates a driver and returns an access token.</summary>
    /// <remarks>Example request: <code>{"email":"driver@example.com","password":"long-password"}</code></remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginDtoIn dto, CancellationToken cancellationToken) =>
        Ok(await auth.LoginAsync(dto, cancellationToken));

    /// <summary>Rotates a refresh token and returns a new access and refresh token pair.</summary>
    /// <remarks>Example request: <code>{"refreshToken":"the-refresh-token"}</code></remarks>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDtoIn dto, CancellationToken cancellationToken) =>
        Ok(await auth.RefreshAsync(dto.RefreshToken, cancellationToken));

    /// <summary>Revokes the family containing the supplied refresh token.</summary>
    /// <remarks>Example request: <code>{"refreshToken":"the-refresh-token"}</code></remarks>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDtoIn dto, CancellationToken cancellationToken)
    {
        await auth.LogoutAsync(dto.RefreshToken, cancellationToken);
        return NoContent();
    }
}
